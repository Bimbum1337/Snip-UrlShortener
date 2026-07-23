using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using UrlShortener.Application.Features.Analytics.Services;
using UrlShortener.Application.Features.Redirection.Services;
using UrlShortener.Application.Features.ShortUrls.Services;

namespace UrlShortener.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ICodeAllocator, CodeAllocator>();
        services.AddScoped<IShortUrlService, ShortUrlService>();
        services.AddScoped<IRedirectService, RedirectService>();
        services.AddScoped<IAnalyticsService, AnalyticsService>();

        services.AddValidatorsFromAssemblyContaining(typeof(DependencyInjection), ServiceLifetime.Singleton);

        return services;
    }
}
