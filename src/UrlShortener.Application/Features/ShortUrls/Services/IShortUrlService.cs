using UrlShortener.Application.Common.Models;
using UrlShortener.Application.Features.ShortUrls.Dtos;
using UrlShortener.Domain.Common;

namespace UrlShortener.Application.Features.ShortUrls.Services;

public interface IShortUrlService
{
    Task<Result<ShortUrlResponse>> CreateAsync(CreateShortUrlRequest request, CancellationToken cancellationToken = default);

    Task<Result<ShortUrlResponse>> GetByCodeAsync(string code, CancellationToken cancellationToken = default);

    Task<Result<PagedResult<ShortUrlResponse>>> ListAsync(ShortUrlQuery query, CancellationToken cancellationToken = default);

    Task<Result<ShortUrlResponse>> UpdateAsync(string code, UpdateShortUrlRequest request, CancellationToken cancellationToken = default);

    Task<Result> DeleteAsync(string code, CancellationToken cancellationToken = default);

    Task<Result<AliasAvailabilityResponse>> CheckAliasAsync(string alias, CancellationToken cancellationToken = default);
}
