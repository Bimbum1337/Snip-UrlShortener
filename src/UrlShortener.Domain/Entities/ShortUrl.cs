using UrlShortener.Domain.Common;
using UrlShortener.Domain.Errors;
using UrlShortener.Domain.ValueObjects;

namespace UrlShortener.Domain.Entities;

public class ShortUrl
{
    private readonly List<UrlVisit> _visits = [];

    private ShortUrl() { }

    private ShortUrl(ShortCode code, DestinationUrl destination, DateTimeOffset createdAtUtc)
    {
        Code = code.Value;
        Destination = destination.Value;
        CreatedAtUtc = createdAtUtc;
        IsActive = true;
    }

    public long Id { get; private set; }

    public string Code { get; private set; } = string.Empty;

    public string Destination { get; private set; } = string.Empty;

    public string? Title { get; private set; }

    public bool IsCustomAlias { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset? ExpiresAtUtc { get; private set; }
    public DateTimeOffset? LastVisitedAtUtc { get; private set; }

    // Denormalised so the list view never has to aggregate UrlVisits.
    public long VisitCount { get; private set; }

    public bool IsActive { get; private set; }

    public IReadOnlyCollection<UrlVisit> Visits => _visits.AsReadOnly();

    public static Result<ShortUrl> Create(
        ShortCode code,
        DestinationUrl destination,
        DateTimeOffset nowUtc,
        string? title = null,
        DateTimeOffset? expiresAtUtc = null,
        bool isCustomAlias = false)
    {
        if (expiresAtUtc is not null && expiresAtUtc <= nowUtc)
            return Result<ShortUrl>.Validation("The expiry date must be in the future.");

        var shortUrl = new ShortUrl(code, destination, nowUtc)
        {
            IsCustomAlias = isCustomAlias,
            ExpiresAtUtc = expiresAtUtc,
        };

        shortUrl.SetTitle(title);

        return Result<ShortUrl>.Success(shortUrl);
    }

    public bool IsExpired(DateTimeOffset nowUtc)
        => ExpiresAtUtc is not null && ExpiresAtUtc <= nowUtc;

    public Result EnsureRedirectable(DateTimeOffset nowUtc)
    {
        if (!IsActive)
            return Result.Failure(ShortUrlErrors.Disabled);

        if (IsExpired(nowUtc))
            return Result.Failure(ShortUrlErrors.Expired);

        return Result.Success();
    }

    // Counter and visit row are written together so they cannot drift apart.
    public void RegisterVisit(DateTimeOffset visitedAtUtc, string? referer, string? userAgent, string? ipHash)
    {
        VisitCount++;
        LastVisitedAtUtc = visitedAtUtc;
        _visits.Add(UrlVisit.Record(Id, visitedAtUtc, referer, userAgent, ipHash));
    }

    public Result UpdateDestination(DestinationUrl destination)
    {
        Destination = destination.Value;
        return Result.Success();
    }

    public void SetTitle(string? title)
    {
        var trimmed = title?.Trim();
        Title = string.IsNullOrWhiteSpace(trimmed) ? null : trimmed;
    }

    public Result SetExpiry(DateTimeOffset? expiresAtUtc, DateTimeOffset nowUtc)
    {
        if (expiresAtUtc is not null && expiresAtUtc <= nowUtc)
            return Result.Validation("The expiry date must be in the future.");

        ExpiresAtUtc = expiresAtUtc;
        return Result.Success();
    }

    public void Activate() => IsActive = true;

    public void Deactivate() => IsActive = false;
}
