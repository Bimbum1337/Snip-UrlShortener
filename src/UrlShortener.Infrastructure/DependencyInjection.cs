using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UrlShortener.Application.Abstractions.Persistence;
using UrlShortener.Application.Abstractions.Services;
using UrlShortener.Application.Common.Options;
using UrlShortener.Infrastructure.Persistence;
using UrlShortener.Infrastructure.Persistence.Repositories;
using UrlShortener.Infrastructure.Services;

namespace UrlShortener.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        bool isDevelopment)
    {
        services.Configure<ShortenerOptions>(configuration.GetSection(ShortenerOptions.SectionName));
        services.Configure<DatabaseOptions>(configuration.GetSection(DatabaseOptions.SectionName));

        var databaseOptions = configuration.GetSection(DatabaseOptions.SectionName).Get<DatabaseOptions>()
                              ?? new DatabaseOptions();

        var connectionString = configuration.GetConnectionString("DbConnectionString")
                               ?? throw new InvalidOperationException(
                                   "Connection string 'DbConnectionString' is not configured.");

        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseSqlServer(connectionString, sql =>
            {
                sql.CommandTimeout(databaseOptions.CommandTimeoutSeconds);
                sql.EnableRetryOnFailure(
                    maxRetryCount: databaseOptions.MaxRetryCount,
                    maxRetryDelay: TimeSpan.FromSeconds(5),
                    errorNumbersToAdd: null);
            });

            if (isDevelopment)
            {
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
            }
        });

        services.AddScoped<IShortUrlRepository, ShortUrlRepository>();
        services.AddScoped<IUrlVisitRepository, UrlVisitRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();
        services.AddSingleton<IShortCodeGenerator, Base62ShortCodeGenerator>();
        services.AddSingleton<IShortLinkBuilder, ShortLinkBuilder>();
        services.AddSingleton<IVisitorHasher, SaltedVisitorHasher>();

        services.AddSingleton<DatabaseInitializer>();

        return services;
    }
}
