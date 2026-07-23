using System.Text.RegularExpressions;
using UrlShortener.Domain.Common;
using UrlShortener.Domain.Errors;

namespace UrlShortener.Domain.ValueObjects;

public readonly partial record struct ShortCode
{
    public const int MinLength = 3;
    public const int MaxLength = 32;

    // Paths the API itself owns, so they can never be handed out as aliases.
    private static readonly HashSet<string> Reserved = new(StringComparer.OrdinalIgnoreCase)
    {
        "api", "admin", "login", "logout", "register", "signup", "signin",
        "health", "healthz", "metrics", "openapi", "swagger", "scalar",
        "static", "assets", "public", "favicon.ico", "robots.txt", "sitemap.xml",
        "new", "edit", "delete", "settings", "about", "help", "docs", "terms",
        "privacy", "u", "s", "r", "go",
    };

    private ShortCode(string value) => Value = value;

    public string Value { get; }

    public override string ToString() => Value;

    public static implicit operator string(ShortCode code) => code.Value;

    public static Result<ShortCode> Create(string? value)
    {
        var candidate = value?.Trim();

        if (string.IsNullOrWhiteSpace(candidate))
            return Result<ShortCode>.Failure(ShortUrlErrors.InvalidAlias("An alias is required."));

        if (candidate.Length < MinLength || candidate.Length > MaxLength)
            return Result<ShortCode>.Failure(ShortUrlErrors.InvalidAlias(
                $"An alias must be between {MinLength} and {MaxLength} characters."));

        if (!AllowedPattern().IsMatch(candidate))
            return Result<ShortCode>.Failure(ShortUrlErrors.InvalidAlias(
                "An alias may only contain letters, digits, hyphens and underscores."));

        if (Reserved.Contains(candidate))
            return Result<ShortCode>.Failure(ShortUrlErrors.InvalidAlias(
                $"'{candidate}' is reserved and cannot be used as an alias."));

        return Result<ShortCode>.Success(new ShortCode(candidate));
    }

    // For values already persisted; skips validation.
    public static ShortCode FromTrustedSource(string value) => new(value);

    [GeneratedRegex("^[A-Za-z0-9_-]+$", RegexOptions.CultureInvariant)]
    private static partial Regex AllowedPattern();
}
