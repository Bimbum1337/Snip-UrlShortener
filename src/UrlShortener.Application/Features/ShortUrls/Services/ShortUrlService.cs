using Microsoft.Extensions.Logging;
using UrlShortener.Application.Abstractions.Persistence;
using UrlShortener.Application.Abstractions.Services;
using UrlShortener.Application.Common.Models;
using UrlShortener.Application.Features.ShortUrls.Dtos;
using UrlShortener.Application.Features.ShortUrls.Mapping;
using UrlShortener.Domain.Common;
using UrlShortener.Domain.Entities;
using UrlShortener.Domain.Errors;
using UrlShortener.Domain.ValueObjects;

namespace UrlShortener.Application.Features.ShortUrls.Services;

public sealed class ShortUrlService(
    IShortUrlRepository repository,
    IUnitOfWork unitOfWork,
    ICodeAllocator codeAllocator,
    IShortLinkBuilder linkBuilder,
    IDateTimeProvider clock,
    ILogger<ShortUrlService> logger) : IShortUrlService
{
    public async Task<Result<ShortUrlResponse>> CreateAsync(
        CreateShortUrlRequest request,
        CancellationToken cancellationToken = default)
    {
        var destinationResult = DestinationUrl.Create(request.Destination);
        if (destinationResult.IsFailure)
            return Result<ShortUrlResponse>.Failure(destinationResult.Errors);

        var destination = destinationResult.Value;

        if (PointsAtShortener(destination))
            return Result<ShortUrlResponse>.Failure(ShortUrlErrors.SelfReferencingUrl);

        var codeResult = await codeAllocator.AllocateAsync(request.CustomAlias, cancellationToken);
        if (codeResult.IsFailure)
            return Result<ShortUrlResponse>.Failure(codeResult.Errors);

        var now = clock.UtcNow;

        var shortUrlResult = ShortUrl.Create(
            codeResult.Value,
            destination,
            now,
            request.Title,
            request.ExpiresAtUtc,
            isCustomAlias: !string.IsNullOrWhiteSpace(request.CustomAlias));

        if (shortUrlResult.IsFailure)
            return Result<ShortUrlResponse>.Failure(shortUrlResult.Errors);

        var shortUrl = shortUrlResult.Value!;

        repository.Add(shortUrl);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Created short link {Code} -> {Destination}", shortUrl.Code, shortUrl.Destination);

        return Result<ShortUrlResponse>.Success(shortUrl.ToResponse(linkBuilder, now));
    }

    public async Task<Result<ShortUrlResponse>> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        var shortUrl = await repository.GetByCodeAsync(code, cancellationToken);

        return shortUrl is null
            ? Result<ShortUrlResponse>.Failure(ShortUrlErrors.NotFound(code))
            : Result<ShortUrlResponse>.Success(shortUrl.ToResponse(linkBuilder, clock.UtcNow));
    }

    public async Task<Result<PagedResult<ShortUrlResponse>>> ListAsync(
        ShortUrlQuery query,
        CancellationToken cancellationToken = default)
    {
        var page = await repository.ListAsync(query, cancellationToken);
        var now = clock.UtcNow;

        return Result<PagedResult<ShortUrlResponse>>.Success(new PagedResult<ShortUrlResponse>(
            page.Items.ToResponses(linkBuilder, now),
            page.Page,
            page.PageSize,
            page.TotalCount));
    }

    public async Task<Result<ShortUrlResponse>> UpdateAsync(
        string code,
        UpdateShortUrlRequest request,
        CancellationToken cancellationToken = default)
    {
        var shortUrl = await repository.GetByCodeAsync(code, cancellationToken);
        if (shortUrl is null)
            return Result<ShortUrlResponse>.Failure(ShortUrlErrors.NotFound(code));

        var now = clock.UtcNow;

        if (request.Destination is not null)
        {
            var destinationResult = DestinationUrl.Create(request.Destination);
            if (destinationResult.IsFailure)
                return Result<ShortUrlResponse>.Failure(destinationResult.Errors);

            if (PointsAtShortener(destinationResult.Value))
                return Result<ShortUrlResponse>.Failure(ShortUrlErrors.SelfReferencingUrl);

            shortUrl.UpdateDestination(destinationResult.Value);
        }

        if (request.Title is not null)
            shortUrl.SetTitle(request.Title);

        if (request.ClearExpiry || request.ExpiresAtUtc is not null)
        {
            var expiryResult = shortUrl.SetExpiry(request.ClearExpiry ? null : request.ExpiresAtUtc, now);
            if (expiryResult.IsFailure)
                return Result<ShortUrlResponse>.Failure(expiryResult.Errors);
        }

        if (request.IsActive is bool isActive)
        {
            if (isActive) shortUrl.Activate();
            else shortUrl.Deactivate();
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Updated short link {Code}", shortUrl.Code);

        return Result<ShortUrlResponse>.Success(shortUrl.ToResponse(linkBuilder, now));
    }

    public async Task<Result> DeleteAsync(string code, CancellationToken cancellationToken = default)
    {
        var shortUrl = await repository.GetByCodeAsync(code, cancellationToken);
        if (shortUrl is null)
            return Result.Failure(ShortUrlErrors.NotFound(code));

        repository.Remove(shortUrl);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Deleted short link {Code}", code);

        return Result.Success();
    }

    public async Task<Result<AliasAvailabilityResponse>> CheckAliasAsync(
        string alias,
        CancellationToken cancellationToken = default)
    {
        var codeResult = ShortCode.Create(alias);
        if (codeResult.IsFailure)
        {
            return Result<AliasAvailabilityResponse>.Success(
                new AliasAvailabilityResponse(alias, false, codeResult.Errors[0].Description));
        }

        var taken = await repository.CodeExistsAsync(codeResult.Value.Value, cancellationToken);

        return Result<AliasAvailabilityResponse>.Success(new AliasAvailabilityResponse(
            codeResult.Value.Value,
            !taken,
            taken ? ShortUrlErrors.CodeAlreadyTaken.Description : null));
    }

    // Stops someone shortening a link that points back at this service.
    private bool PointsAtShortener(DestinationUrl destination)
    {
        var baseUrl = linkBuilder.BuildAbsoluteUrl(string.Empty);

        return Uri.TryCreate(baseUrl, UriKind.Absolute, out var baseUri)
            && string.Equals(destination.Authority, baseUri.Authority, StringComparison.OrdinalIgnoreCase);
    }
}
