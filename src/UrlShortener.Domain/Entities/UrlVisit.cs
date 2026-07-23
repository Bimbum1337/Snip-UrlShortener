namespace UrlShortener.Domain.Entities;

public class UrlVisit
{
    private UrlVisit() { }

    public long Id { get; private set; }

    public long ShortUrlId { get; private set; }

    public DateTimeOffset VisitedAtUtc { get; private set; }

    public string? Referer { get; private set; }

    public string? UserAgent { get; private set; }

    // Hashed, not the raw address - enough to count unique visitors.
    public string? IpHash { get; private set; }

    public ShortUrl? ShortUrl { get; private set; }

    internal static UrlVisit Record(
        long shortUrlId,
        DateTimeOffset visitedAtUtc,
        string? referer,
        string? userAgent,
        string? ipHash)
        => new()
        {
            ShortUrlId = shortUrlId,
            VisitedAtUtc = visitedAtUtc,
            Referer = Truncate(referer, 512),
            UserAgent = Truncate(userAgent, 512),
            IpHash = Truncate(ipHash, 64),
        };

    private static string? Truncate(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        return value.Length <= maxLength ? value : value[..maxLength];
    }
}
