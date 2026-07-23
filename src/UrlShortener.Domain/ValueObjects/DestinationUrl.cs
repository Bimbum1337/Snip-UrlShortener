using System.Text.RegularExpressions;
using UrlShortener.Domain.Common;
using UrlShortener.Domain.Errors;

namespace UrlShortener.Domain.ValueObjects;

public readonly partial record struct DestinationUrl
{
    public const int MaxLength = 2048;

    private DestinationUrl(string value) => Value = value;

    public string Value { get; }

    public string Host => new Uri(Value).Host;

    public string Authority => new Uri(Value).Authority;

    public override string ToString() => Value;

    public static implicit operator string(DestinationUrl url) => url.Value;

    public static Result<DestinationUrl> Create(string? value)
    {
        var candidate = value?.Trim();

        if (string.IsNullOrWhiteSpace(candidate))
            return Result<DestinationUrl>.Failure(ShortUrlErrors.InvalidUrl("A destination URL is required."));

        // "example.com" is a fair thing to paste, so assume https when no scheme
        // was given. Anything that does name a scheme falls through to the check
        // below and gets a proper message.
        if (!SchemePattern().IsMatch(candidate))
            candidate = "https://" + candidate;

        if (candidate.Length > MaxLength)
            return Result<DestinationUrl>.Failure(ShortUrlErrors.InvalidUrl(
                $"A destination URL cannot exceed {MaxLength} characters."));

        if (!Uri.TryCreate(candidate, UriKind.Absolute, out var uri))
            return Result<DestinationUrl>.Failure(ShortUrlErrors.InvalidUrl("That does not look like a valid URL."));

        // Only http(s). javascript:, data: and file: would all be redirect targets otherwise.
        if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
            return Result<DestinationUrl>.Failure(ShortUrlErrors.InvalidUrl("Only http and https URLs can be shortened."));

        if (string.IsNullOrWhiteSpace(uri.Host) || !uri.Host.Contains('.') && !uri.IsLoopback)
            return Result<DestinationUrl>.Failure(ShortUrlErrors.InvalidUrl("The URL must include a valid host name."));

        return Result<DestinationUrl>.Success(new DestinationUrl(uri.ToString()));
    }

    public static DestinationUrl FromTrustedSource(string value) => new(value);

    [GeneratedRegex("^[a-zA-Z][a-zA-Z0-9+.-]*:", RegexOptions.CultureInvariant)]
    private static partial Regex SchemePattern();
}
