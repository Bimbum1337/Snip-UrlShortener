using Microsoft.Extensions.Options;
using UrlShortener.Application.Abstractions.Persistence;
using UrlShortener.Application.Abstractions.Services;
using UrlShortener.Application.Common.Options;
using UrlShortener.Domain.Common;
using UrlShortener.Domain.Errors;
using UrlShortener.Domain.ValueObjects;

namespace UrlShortener.Application.Features.ShortUrls.Services;

public sealed class CodeAllocator(
    IShortUrlRepository repository,
    IShortCodeGenerator generator,
    IOptions<ShortenerOptions> options) : ICodeAllocator
{
    private readonly ShortenerOptions _options = options.Value;

    public async Task<Result<ShortCode>> AllocateAsync(string? requestedAlias, CancellationToken cancellationToken = default)
        => string.IsNullOrWhiteSpace(requestedAlias)
            ? await GenerateUnusedAsync(cancellationToken)
            : await ReserveAliasAsync(requestedAlias, cancellationToken);

    private async Task<Result<ShortCode>> ReserveAliasAsync(string alias, CancellationToken cancellationToken)
    {
        var codeResult = ShortCode.Create(alias);
        if (codeResult.IsFailure)
            return codeResult;

        var code = codeResult.Value;

        if (await repository.CodeExistsAsync(code.Value, cancellationToken))
            return Result<ShortCode>.Failure(ShortUrlErrors.CodeAlreadyTaken);

        return Result<ShortCode>.Success(code);
    }

    private async Task<Result<ShortCode>> GenerateUnusedAsync(CancellationToken cancellationToken)
    {
        var length = _options.CodeLength;

        for (var attempt = 0; attempt < _options.MaxGenerationAttempts; attempt++)
        {
            // Collisions are rare; if we do hit a few, widen the space rather
            // than keep drawing from the same one.
            if (attempt > 0 && attempt % 3 == 0)
                length++;

            var candidate = generator.Generate(length);

            var codeResult = ShortCode.Create(candidate);
            if (codeResult.IsFailure)
                continue;

            if (!await repository.CodeExistsAsync(candidate, cancellationToken))
                return codeResult;
        }

        return Result<ShortCode>.Failure(ShortUrlErrors.CodeGenerationFailed);
    }
}
