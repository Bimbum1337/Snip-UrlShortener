using Microsoft.EntityFrameworkCore;
using UrlShortener.Application.Abstractions.Persistence;
using UrlShortener.Application.Common.Models;
using UrlShortener.Domain.Entities;

namespace UrlShortener.Infrastructure.Persistence.Repositories;

public sealed class ShortUrlRepository(AppDbContext context) : IShortUrlRepository
{
    public Task<ShortUrl?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
        => context.ShortUrls.FirstOrDefaultAsync(x => x.Code == code, cancellationToken);

    public Task<bool> CodeExistsAsync(string code, CancellationToken cancellationToken = default)
        => context.ShortUrls.AsNoTracking().AnyAsync(x => x.Code == code, cancellationToken);

    public async Task<PagedResult<ShortUrl>> ListAsync(ShortUrlQuery query, CancellationToken cancellationToken = default)
    {
        var source = context.ShortUrls.AsNoTracking().AsQueryable();

        source = ApplyFilters(source, query);

        var total = await source.LongCountAsync(cancellationToken);
        if (total == 0)
            return PagedResult<ShortUrl>.Empty(query.Page, query.PageSize);

        var items = await ApplySort(source, query)
            .Skip(query.Skip)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<ShortUrl>(items, query.Page, query.PageSize, total);
    }

    public Task<ShortUrl?> GetMostVisitedAsync(CancellationToken cancellationToken = default)
        => context.ShortUrls
            .AsNoTracking()
            .OrderByDescending(x => x.VisitCount)
            .FirstOrDefaultAsync(cancellationToken);

    public Task<long> CountAsync(bool? isActive = null, CancellationToken cancellationToken = default)
        => context.ShortUrls
            .AsNoTracking()
            .Where(x => isActive == null || x.IsActive == isActive)
            .LongCountAsync(cancellationToken);

    public void Add(ShortUrl shortUrl) => context.ShortUrls.Add(shortUrl);

    public void Remove(ShortUrl shortUrl) => context.ShortUrls.Remove(shortUrl);

    private static IQueryable<ShortUrl> ApplyFilters(IQueryable<ShortUrl> source, ShortUrlQuery query)
    {
        if (query.IsActive is bool isActive)
            source = source.Where(x => x.IsActive == isActive);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = $"%{query.Search.Trim()}%";

            source = source.Where(x =>
                EF.Functions.Like(x.Code, term)
                || EF.Functions.Like(x.Destination, term)
                || (x.Title != null && EF.Functions.Like(x.Title, term)));
        }

        return source;
    }

    private static IQueryable<ShortUrl> ApplySort(IQueryable<ShortUrl> source, ShortUrlQuery query)
        => (query.SortBy, query.Descending) switch
        {
            (ShortUrlSortBy.Visits, true) => source.OrderByDescending(x => x.VisitCount).ThenByDescending(x => x.Id),
            (ShortUrlSortBy.Visits, false) => source.OrderBy(x => x.VisitCount).ThenBy(x => x.Id),
            (ShortUrlSortBy.LastVisitedAt, true) => source.OrderByDescending(x => x.LastVisitedAtUtc).ThenByDescending(x => x.Id),
            (ShortUrlSortBy.LastVisitedAt, false) => source.OrderBy(x => x.LastVisitedAtUtc).ThenBy(x => x.Id),
            (ShortUrlSortBy.Code, true) => source.OrderByDescending(x => x.Code),
            (ShortUrlSortBy.Code, false) => source.OrderBy(x => x.Code),
            (_, false) => source.OrderBy(x => x.CreatedAtUtc).ThenBy(x => x.Id),
            _ => source.OrderByDescending(x => x.CreatedAtUtc).ThenByDescending(x => x.Id),
        };
}
