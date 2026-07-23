using UrlShortener.Domain.Common;
using UrlShortener.Domain.ValueObjects;

namespace UrlShortener.Application.Features.ShortUrls.Services;

public interface ICodeAllocator
{
    Task<Result<ShortCode>> AllocateAsync(string? requestedAlias, CancellationToken cancellationToken = default);
}
