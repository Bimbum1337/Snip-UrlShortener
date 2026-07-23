using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace UrlShortener.Infrastructure.Persistence;

public sealed class DatabaseInitializer(
    IServiceProvider serviceProvider,
    IOptions<DatabaseOptions> options,
    ILogger<DatabaseInitializer> logger)
{
    public async Task InitialiseAsync(CancellationToken cancellationToken = default)
    {
        if (!options.Value.MigrateOnStartup)
            return;

        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        try
        {
            await context.Database.MigrateAsync(cancellationToken);
            logger.LogInformation("Database migrations applied");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Database initialisation failed");
            throw;
        }
    }
}
