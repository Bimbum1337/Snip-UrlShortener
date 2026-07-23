namespace UrlShortener.Application.Features.ShortUrls.Dtos;

public sealed record CreateShortUrlRequest(
    string Destination,
    string? CustomAlias = null,
    string? Title = null,
    DateTimeOffset? ExpiresAtUtc = null);

// Only the fields that are supplied get applied.
public sealed record UpdateShortUrlRequest(
    string? Destination = null,
    string? Title = null,
    DateTimeOffset? ExpiresAtUtc = null,
    bool? IsActive = null,
    bool ClearExpiry = false);

public sealed record ShortUrlResponse(
    long Id,
    string Code,
    string ShortLink,
    string Destination,
    string? Title,
    bool IsCustomAlias,
    bool IsActive,
    bool IsExpired,
    long VisitCount,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? ExpiresAtUtc,
    DateTimeOffset? LastVisitedAtUtc);

public sealed record AliasAvailabilityResponse(string Alias, bool IsAvailable, string? Reason);
