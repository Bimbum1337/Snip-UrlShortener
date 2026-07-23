namespace UrlShortener.Domain.Common;

public static class ErrorCodes
{
    public const string NotFound = "NotFound";
    public const string Validation = "Validation";
    public const string BadRequest = "BadRequest";
    public const string Conflict = "Conflict";
    public const string Failure = "Failure";
    public const string Forbid = "Forbid";
}

public readonly record struct Error(string Code, string Description)
{
    public static readonly Error None = new(string.Empty, string.Empty);

    public bool IsNone => string.IsNullOrWhiteSpace(Code);

    public static Error NotFound(string description) => new(ErrorCodes.NotFound, description);
    public static Error Validation(string description) => new(ErrorCodes.Validation, description);
    public static Error BadRequest(string description) => new(ErrorCodes.BadRequest, description);
    public static Error Conflict(string description) => new(ErrorCodes.Conflict, description);
    public static Error Failure(string description) => new(ErrorCodes.Failure, description);
    public static Error Forbid(string description) => new(ErrorCodes.Forbid, description);
}
