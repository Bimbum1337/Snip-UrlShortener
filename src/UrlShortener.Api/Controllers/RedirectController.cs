using Microsoft.AspNetCore.Mvc;
using UrlShortener.Application.Features.Redirection.Dtos;
using UrlShortener.Application.Features.Redirection.Services;

namespace UrlShortener.Api.Controllers;

// Sits at the root so links stay short. The "api" prefix is reserved to keep
// the two surfaces apart.
[ApiController]
public sealed class RedirectController(IRedirectService redirects) : BaseApiController
{
    [HttpGet("/{code}")]
    [ProducesResponseType(StatusCodes.Status302Found)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult> Follow(string code, CancellationToken cancellationToken)
    {
        var context = new VisitContext(
            Request.Headers.Referer.ToString() is { Length: > 0 } referer ? referer : null,
            Request.Headers.UserAgent.ToString() is { Length: > 0 } agent ? agent : null,
            HttpContext.Connection.RemoteIpAddress?.ToString());

        var result = await redirects.ResolveAsync(code, context, cancellationToken);

        if (result.IsFailure)
            return MapErrorsToResponse(result.Errors);

        // 302, not 301: a permanent redirect gets cached by the browser and the
        // visit stops being counted.
        return Redirect(result.Value!.Destination);
    }

    [HttpGet("/api/preview/{code}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RedirectTarget>> Preview(string code, CancellationToken cancellationToken)
        => ToActionResult(await redirects.PeekAsync(code, cancellationToken));
}
