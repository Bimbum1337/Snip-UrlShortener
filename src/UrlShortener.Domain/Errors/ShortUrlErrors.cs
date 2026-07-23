using UrlShortener.Domain.Common;

namespace UrlShortener.Domain.Errors;

public static class ShortUrlErrors
{
    public static Error NotFound(string code)
        => Error.NotFound($"No short link exists for code '{code}'.");

    public static readonly Error CodeAlreadyTaken
        = Error.Conflict("That custom alias is already in use. Try a different one.");

    public static readonly Error CodeGenerationFailed
        = Error.Failure("Could not allocate a unique short code. Please retry.");

    public static readonly Error Expired
        = Error.NotFound("This short link has expired.");

    public static readonly Error Disabled
        = Error.Forbid("This short link has been disabled.");

    public static Error InvalidUrl(string reason) => Error.Validation(reason);

    public static Error InvalidAlias(string reason) => Error.Validation(reason);

    public static readonly Error SelfReferencingUrl
        = Error.Validation("The destination cannot point back at the shortener itself.");
}
