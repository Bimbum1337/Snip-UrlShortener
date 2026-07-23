using UrlShortener.Application.Features.Analytics.Dtos;

namespace UrlShortener.Application.Abstractions.Persistence;

public interface IUrlVisitRepository
{
    Task<IReadOnlyList<DailyVisitCountDto>> GetDailyVisitCountsAsync(
        long shortUrlId,
        DateTimeOffset fromUtc,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ReferrerCountDto>> GetTopReferrersAsync(
        long shortUrlId,
        int take,
        CancellationToken cancellationToken = default);

    Task<long> GetUniqueVisitorCountAsync(long shortUrlId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DailyVisitCountDto>> GetDailyVisitCountsForAllAsync(
        DateTimeOffset fromUtc,
        CancellationToken cancellationToken = default);

    Task<long> GetTotalVisitCountAsync(CancellationToken cancellationToken = default);
}
