using UrlShortener.Application.Features.Analytics.Dtos;
using UrlShortener.Domain.Common;

namespace UrlShortener.Application.Features.Analytics.Services;

public interface IAnalyticsService
{
    Task<Result<ShortUrlStatsResponse>> GetStatsAsync(
        string code,
        int? windowDays = null,
        CancellationToken cancellationToken = default);

    Task<Result<DashboardSummaryResponse>> GetSummaryAsync(CancellationToken cancellationToken = default);
}
