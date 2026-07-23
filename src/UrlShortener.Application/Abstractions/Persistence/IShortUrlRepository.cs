using UrlShortener.Application.Common.Models;
using UrlShortener.Domain.Entities;

namespace UrlShortener.Application.Abstractions.Persistence;

public interface IShortUrlRepository
{
    // Change tracked - callers may modify the returned aggregate.
    Task<ShortUrl?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);

    Task<bool> CodeExistsAsync(string code, CancellationToken cancellationToken = default);

    Task<PagedResult<ShortUrl>> ListAsync(ShortUrlQuery query, CancellationToken cancellationToken = default);

    Task<ShortUrl?> GetMostVisitedAsync(CancellationToken cancellationToken = default);

    Task<long> CountAsync(bool? isActive = null, CancellationToken cancellationToken = default);

    void Add(ShortUrl shortUrl);

    void Remove(ShortUrl shortUrl);
}
