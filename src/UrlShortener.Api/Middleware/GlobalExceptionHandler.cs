using System.Diagnostics;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace UrlShortener.Api.Middleware;

public sealed class GlobalExceptionHandler(
    IHostEnvironment environment,
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var traceId = Activity.Current?.Id ?? httpContext.TraceIdentifier;

        logger.LogError(
            exception,
            "Unhandled exception. TraceId: {TraceId}, Path: {Path}, Method: {Method}",
            traceId,
            httpContext.Request.Path,
            httpContext.Request.Method);

        var problemDetails = new ProblemDetails
        {
            Title = "An error occurred while processing your request",
            Status = StatusCodes.Status500InternalServerError,
            Type = "https://datatracker.ietf.org/doc/html/rfc9110#section-15.6.1",
            Instance = httpContext.Request.Path,
            Detail = environment.IsDevelopment()
                ? exception.Message
                : "An unexpected error occurred. Please try again later.",
        };

        problemDetails.Extensions["traceId"] = traceId;

        if (environment.IsDevelopment())
        {
            problemDetails.Extensions["exceptionType"] = exception.GetType().Name;
            problemDetails.Extensions["stackTrace"] = exception.StackTrace;
        }

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
        httpContext.Response.ContentType = "application/problem+json";

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}
