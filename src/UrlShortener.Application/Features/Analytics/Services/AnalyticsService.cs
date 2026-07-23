using Microsoft.Extensions.Options;
using UrlShortener.Application.Abstractions.Persistence;
using UrlShortener.Application.Abstractions.Services;
using UrlShortener.Application.Common.Options;
using UrlShortener.Application.Features.Analytics.Dtos;
using UrlShortener.Domain.Common;
using UrlShortener.Domain.Errors;

namespace UrlShortener.Application.Features.Analytics.Services;

public sealed class AnalyticsService(
    IShortUrlRepository shortUrls,
    IUrlVisitRepository visits,
    IShortLinkBuilder linkBuilder,
    IDateTimeProvider clock,
    IOptions<ShortenerOptions> options) : IAnalyticsService
{
    private const int TopReferrerCount = 5;

    private readonly ShortenerOptions _options = options.Value;

    public async Task<Result<ShortUrlStatsResponse>> GetStatsAsync(
        string code,
        int? windowDays = null,
        CancellationToken cancellationToken = default)
    {
        var shortUrl = await shortUrls.GetByCodeAsync(code, cancellationToken);
        if (shortUrl is null)
            return Result<ShortUrlStatsResponse>.Failure(ShortUrlErrors.NotFound(code));

        var days = NormaliseWindow(windowDays);
        var from = clock.UtcNow.Date.AddDays(-(days - 1));

        var timeline = await visits.GetDailyVisitCountsAsync(shortUrl.Id, from, cancellationToken);
        var referrers = await visits.GetTopReferrersAsync(shortUrl.Id, TopReferrerCount, cancellationToken);
        var unique = await visits.GetUniqueVisitorCountAsync(shortUrl.Id, cancellationToken);

        return Result<ShortUrlStatsResponse>.Success(new ShortUrlStatsResponse(
            shortUrl.Code,
            linkBuilder.BuildAbsoluteUrl(shortUrl.Code),
            shortUrl.Destination,
            shortUrl.VisitCount,
            unique,
            shortUrl.LastVisitedAtUtc,
            FillGaps(timeline, DateOnly.FromDateTime(from), days),
            referrers));
    }

    public async Task<Result<DashboardSummaryResponse>> GetSummaryAsync(CancellationToken cancellationToken = default)
    {
        var days = _options.AnalyticsWindowDays;
        var from = clock.UtcNow.Date.AddDays(-(days - 1));

        var totalLinks = await shortUrls.CountAsync(cancellationToken: cancellationToken);
        var activeLinks = await shortUrls.CountAsync(isActive: true, cancellationToken);
        var totalVisits = await visits.GetTotalVisitCountAsync(cancellationToken);
        var timeline = FillGaps(
            await visits.GetDailyVisitCountsForAllAsync(from, cancellationToken),
            DateOnly.FromDateTime(from),
            days);

        var last7 = timeline
            .Where(d => d.Date >= DateOnly.FromDateTime(clock.UtcNow.Date.AddDays(-6)))
            .Sum(d => d.Count);

        var top = await shortUrls.GetMostVisitedAsync(cancellationToken);

        return Result<DashboardSummaryResponse>.Success(new DashboardSummaryResponse(
            totalLinks,
            activeLinks,
            totalVisits,
            last7,
            top?.Code,
            top is null ? null : linkBuilder.BuildAbsoluteUrl(top.Code),
            top?.VisitCount ?? 0,
            timeline));
    }

    private int NormaliseWindow(int? windowDays) => windowDays switch
    {
        null or < 1 => _options.AnalyticsWindowDays,
        > 365 => 365,
        _ => windowDays.Value,
    };

    // The query only returns days that had traffic, but a chart needs a point
    // per day, so pad the rest with zero.
    private static IReadOnlyList<DailyVisitCountDto> FillGaps(
        IReadOnlyList<DailyVisitCountDto> source,
        DateOnly from,
        int days)
    {
        var lookup = source.ToDictionary(d => d.Date, d => d.Count);

        return Enumerable.Range(0, days)
            .Select(offset =>
            {
                var date = from.AddDays(offset);
                return new DailyVisitCountDto(date, lookup.GetValueOrDefault(date));
            })
            .ToList();
    }
}
