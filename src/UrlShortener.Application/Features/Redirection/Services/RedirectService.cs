using Microsoft.Extensions.Logging;
using UrlShortener.Application.Abstractions.Persistence;
using UrlShortener.Application.Abstractions.Services;
using UrlShortener.Application.Features.Redirection.Dtos;
using UrlShortener.Domain.Common;
using UrlShortener.Domain.Errors;

namespace UrlShortener.Application.Features.Redirection.Services;

public sealed class RedirectService(
    IShortUrlRepository repository,
    IUnitOfWork unitOfWork,
    IVisitorHasher visitorHasher,
    IDateTimeProvider clock,
    ILogger<RedirectService> logger) : IRedirectService
{
    public async Task<Result<RedirectTarget>> ResolveAsync(
        string code,
        VisitContext context,
        CancellationToken cancellationToken = default)
    {
        var shortUrl = await repository.GetByCodeAsync(code, cancellationToken);
        if (shortUrl is null)
            return Result<RedirectTarget>.Failure(ShortUrlErrors.NotFound(code));

        var now = clock.UtcNow;

        var redirectable = shortUrl.EnsureRedirectable(now);
        if (redirectable.IsFailure)
            return Result<RedirectTarget>.Failure(redirectable.Errors);

        shortUrl.RegisterVisit(now, context.Referer, context.UserAgent, visitorHasher.Hash(context.IpAddress));

        // A failed analytics write should not cost the visitor their redirect.
        try
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to record visit for {Code}; redirecting anyway", code);
        }

        return Result<RedirectTarget>.Success(new RedirectTarget(shortUrl.Destination, shortUrl.Code));
    }

    public async Task<Result<RedirectTarget>> PeekAsync(string code, CancellationToken cancellationToken = default)
    {
        var shortUrl = await repository.GetByCodeAsync(code, cancellationToken);
        if (shortUrl is null)
            return Result<RedirectTarget>.Failure(ShortUrlErrors.NotFound(code));

        var redirectable = shortUrl.EnsureRedirectable(clock.UtcNow);

        return redirectable.IsFailure
            ? Result<RedirectTarget>.Failure(redirectable.Errors)
            : Result<RedirectTarget>.Success(new RedirectTarget(shortUrl.Destination, shortUrl.Code));
    }
}
