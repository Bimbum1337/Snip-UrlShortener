using Microsoft.AspNetCore.Mvc;
using UrlShortener.Application.Common.Models;
using UrlShortener.Application.Features.ShortUrls.Dtos;
using UrlShortener.Application.Features.ShortUrls.Services;

namespace UrlShortener.Api.Controllers;

[ApiController]
[Route("api/urls")]
[Produces("application/json")]
public sealed class ShortUrlsController(IShortUrlService shortUrls) : BaseApiController
{
    public const string GetByCodeRouteName = "GetShortUrlByCode";

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ShortUrlResponse>> Create(
        [FromBody] CreateShortUrlRequest request,
        CancellationToken cancellationToken)
    {
        var result = await shortUrls.CreateAsync(request, cancellationToken);

        return ToCreatedResult(
            result,
            GetByCodeRouteName,
            new { code = result.IsSuccess ? result.Value!.Code : string.Empty });
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<ShortUrlResponse>>> List(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] ShortUrlSortBy sortBy = ShortUrlSortBy.CreatedAt,
        [FromQuery] bool desc = true,
        CancellationToken cancellationToken = default)
    {
        var query = new ShortUrlQuery
        {
            Page = page,
            PageSize = pageSize,
            Search = search,
            IsActive = isActive,
            SortBy = sortBy,
            Descending = desc,
        };

        return ToActionResult(await shortUrls.ListAsync(query, cancellationToken));
    }

    [HttpGet("{code}", Name = GetByCodeRouteName)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ShortUrlResponse>> GetByCode(string code, CancellationToken cancellationToken)
        => ToActionResult(await shortUrls.GetByCodeAsync(code, cancellationToken));

    [HttpPatch("{code}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ShortUrlResponse>> Update(
        string code,
        [FromBody] UpdateShortUrlRequest request,
        CancellationToken cancellationToken)
        => ToActionResult(await shortUrls.UpdateAsync(code, request, cancellationToken));

    [HttpDelete("{code}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Delete(string code, CancellationToken cancellationToken)
        => ToActionResult(await shortUrls.DeleteAsync(code, cancellationToken));

    [HttpGet("alias-available/{alias}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<AliasAvailabilityResponse>> CheckAlias(
        string alias,
        CancellationToken cancellationToken)
        => ToActionResult(await shortUrls.CheckAliasAsync(alias, cancellationToken));
}
