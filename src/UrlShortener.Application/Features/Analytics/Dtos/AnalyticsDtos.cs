namespace UrlShortener.Application.Features.Analytics.Dtos;

public sealed record DailyVisitCountDto(DateOnly Date, long Count);

public sealed record ReferrerCountDto(string Referrer, long Count);

public sealed record ShortUrlStatsResponse(
    string Code,
    string ShortLink,
    string Destination,
    long TotalVisits,
    long UniqueVisitors,
    DateTimeOffset? LastVisitedAtUtc,
    IReadOnlyList<DailyVisitCountDto> Timeline,
    IReadOnlyList<ReferrerCountDto> TopReferrers);

public sealed record DashboardSummaryResponse(
    long TotalLinks,
    long ActiveLinks,
    long TotalVisits,
    long VisitsLast7Days,
    string? TopCode,
    string? TopShortLink,
    long TopCodeVisits,
    IReadOnlyList<DailyVisitCountDto> Timeline);
