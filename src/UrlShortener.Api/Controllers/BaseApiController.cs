using Microsoft.AspNetCore.Mvc;
using UrlShortener.Domain.Common;

namespace UrlShortener.Api.Controllers;

public abstract class BaseApiController : ControllerBase
{
    protected ActionResult<T> ToActionResult<T>(Result<T> result)
        => result.IsSuccess ? Ok(result.Value) : MapErrorsToResponse(result.Errors);

    protected ActionResult ToActionResult(Result result)
        => result.IsSuccess ? NoContent() : MapErrorsToResponse(result.Errors);

    protected ActionResult<T> ToCreatedResult<T>(Result<T> result, string routeName, object routeValues)
        => result.IsSuccess
            ? CreatedAtRoute(routeName, routeValues, result.Value)
            : MapErrorsToResponse(result.Errors);

    // The one place that turns a domain error code into an HTTP status.
    protected ActionResult MapErrorsToResponse(Error[] errors)
    {
        if (errors is null || errors.Length == 0)
        {
            return Problem(
                statusCode: StatusCodes.Status500InternalServerError,
                title: "An error occurred",
                detail: "No error details were provided.");
        }

        var detail = string.Join("; ", errors.Select(e => e.Description));

        return errors[0].Code switch
        {
            ErrorCodes.NotFound => Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Resource not found",
                detail: detail),

            ErrorCodes.Validation => Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Validation failed",
                detail: detail),

            ErrorCodes.BadRequest => Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Bad request",
                detail: detail),

            ErrorCodes.Conflict => Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "Conflict",
                detail: detail),

            ErrorCodes.Forbid => Problem(
                statusCode: StatusCodes.Status403Forbidden,
                title: "Forbidden",
                detail: detail),

            _ => Problem(
                statusCode: StatusCodes.Status500InternalServerError,
                title: "Unexpected error",
                detail: detail),
        };
    }
}
