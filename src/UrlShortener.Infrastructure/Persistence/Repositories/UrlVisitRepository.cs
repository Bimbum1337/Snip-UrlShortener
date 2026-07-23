using Microsoft.EntityFrameworkCore;
using UrlShortener.Application.Abstractions.Persistence;
using UrlShortener.Application.Features.Analytics.Dtos;
using UrlShortener.Domain.Entities;

namespace UrlShortener.Infrastructure.Persistence.Repositories;

public sealed class UrlVisitRepository(AppDbContext context) : IUrlVisitRepository
{
    public async Task<IReadOnlyList<DailyVisitCountDto>> GetDailyVisitCountsAsync(
        long shortUrlId,
        DateTimeOffset fromUtc,
        CancellationToken cancellationToken = default)
        => await AggregateByDayAsync(
            context.UrlVisits.AsNoTracking().Where(v => v.ShortUrlId == shortUrlId && v.VisitedAtUtc >= fromUtc),
            cancellationToken);

    public async Task<IReadOnlyList<DailyVisitCountDto>> GetDailyVisitCountsForAllAsync(
        DateTimeOffset fromUtc,
        CancellationToken cancellationToken = default)
        => await AggregateByDayAsync(
            context.UrlVisits.AsNoTracking().Where(v => v.VisitedAtUtc >= fromUtc),
            cancellationToken);

    public async Task<IReadOnlyList<ReferrerCountDto>> GetTopReferrersAsync(
        long shortUrlId,
        int take,
        CancellationToken cancellationToken = default)
    {
        var rows = await context.UrlVisits
            .AsNoTracking()
            .Where(v => v.ShortUrlId == shortUrlId)
            .GroupBy(v => v.Referer)
            .Select(g => new { Referrer = g.Key, Count = g.LongCount() })
            .OrderByDescending(x => x.Count)
            .Take(take)
            .ToListAsync(cancellationToken);

        return rows
            .Select(r => new ReferrerCountDto(NormaliseReferrer(r.Referrer), r.Count))
            .ToList();
    }

    public Task<long> GetUniqueVisitorCountAsync(long shortUrlId, CancellationToken cancellationToken = default)
        => context.UrlVisits
            .AsNoTracking()
            .Where(v => v.ShortUrlId == shortUrlId && v.IpHash != null)
            .Select(v => v.IpHash)
            .Distinct()
            .LongCountAsync(cancellationToken);

    public Task<long> GetTotalVisitCountAsync(CancellationToken cancellationToken = default)
        => context.UrlVisits.AsNoTracking().LongCountAsync(cancellationToken);

    // Grouped on Year/Month/Day rather than .Date, which does not translate for
    // DateTimeOffset - this keeps the aggregation in SQL.
    private static async Task<IReadOnlyList<DailyVisitCountDto>> AggregateByDayAsync(
        IQueryable<UrlVisit> source,
        CancellationToken cancellationToken)
    {
        var rows = await source
            .GroupBy(v => new { v.VisitedAtUtc.Year, v.VisitedAtUtc.Month, v.VisitedAtUtc.Day })
            .Select(g => new
            {
                g.Key.Year,
                g.Key.Month,
                g.Key.Day,
                Count = g.LongCount(),
            })
            .ToListAsync(cancellationToken);

        return rows
            .Select(r => new DailyVisitCountDto(new DateOnly(r.Year, r.Month, r.Day), r.Count))
            .OrderBy(r => r.Date)
            .ToList();
    }

    private static string NormaliseReferrer(string? referer)
    {
        if (string.IsNullOrWhiteSpace(referer))
            return "Direct";

        return Uri.TryCreate(referer, UriKind.Absolute, out var uri) ? uri.Host : referer;
    }
}
