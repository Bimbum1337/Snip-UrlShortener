namespace UrlShortener.Infrastructure.Persistence;

public sealed class DatabaseOptions
{
    public const string SectionName = "Database";

    public int CommandTimeoutSeconds { get; set; } = 30;

    public int MaxRetryCount { get; set; } = 3;

    // Convenient in development; turn off where migrations are deployed separately.
    public bool MigrateOnStartup { get; set; } = true;
}
