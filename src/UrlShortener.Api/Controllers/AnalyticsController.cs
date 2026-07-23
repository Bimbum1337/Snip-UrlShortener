using Microsoft.AspNetCore.Mvc;
using UrlShortener.Application.Features.Analytics.Dtos;
using UrlShortener.Application.Features.Analytics.Services;

namespace UrlShortener.Api.Controllers;

[ApiController]
[Route("api/analytics")]
[Produces("application/json")]
public sealed class AnalyticsController(IAnalyticsService analytics) : BaseApiController
{
    [HttpGet("summary")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<DashboardSummaryResponse>> Summary(CancellationToken cancellationToken)
        => ToActionResult(await analytics.GetSummaryAsync(cancellationToken));

    [HttpGet("{code}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ShortUrlStatsResponse>> ForCode(
        string code,
        [FromQuery] int? days = null,
        CancellationToken cancellationToken = default)
        => ToActionResult(await analytics.GetStatsAsync(code, days, cancellationToken));
}
