namespace UrlShortener.Application.Common.Options;

public sealed class ShortenerOptions
{
    public const string SectionName = "Shortener";

    // Public origin used to build absolute short links, e.g. "https://sho.rt".
    public string BaseUrl { get; set; } = "http://localhost:5219";

    public int CodeLength { get; set; } = 7;

    public int MaxGenerationAttempts { get; set; } = 6;

    public int AnalyticsWindowDays { get; set; } = 30;

    public string VisitorHashSalt { get; set; } = "url-shortener-default-salt";
}
