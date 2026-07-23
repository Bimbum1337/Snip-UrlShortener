namespace UrlShortener.Application.Features.Redirection.Dtos;

// Supplied by the API layer so nothing below it needs HttpContext.
public sealed record VisitContext(string? Referer, string? UserAgent, string? IpAddress)
{
    public static readonly VisitContext Unknown = new(null, null, null);
}

public sealed record RedirectTarget(string Destination, string Code);
