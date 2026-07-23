using System.Text.Json.Serialization;
using UrlShortener.Api.Filters;
using UrlShortener.Api.Middleware;

namespace UrlShortener.Api.Extensions;

public static class ApiServiceCollectionExtensions
{
    public const string FrontendCorsPolicy = "Frontend";

    public static IServiceCollection AddApiLayer(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers(options => options.Filters.Add<FluentValidationFilter>())
            .AddJsonOptions(options =>
            {
                // Enum names rather than numbers, so the client does not have to
                // mirror the ordinals.
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            });

        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();
        services.AddOpenApi();

        var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                             ?? ["http://localhost:5173"];

        services.AddCors(options => options.AddPolicy(FrontendCorsPolicy, policy => policy
            .WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()));

        return services;
    }
}
