using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;

namespace SprintUp.Shared.Infrastructure;

/// <summary>
/// Enforces antiforgery validation for state-changing Minimal API endpoints that opt in with
/// <see cref="IAntiforgeryMetadata"/>, and returns the shared ProblemDetails shape with the
/// stable <c>antiforgery_failed</c> code instead of a bare 400. Blazor's built-in validation
/// remains in place through <c>UseAntiforgery</c>.
/// </summary>
internal sealed class AntiforgeryValidationMiddleware(
    RequestDelegate next,
    IAntiforgery antiforgery,
    ILogger<AntiforgeryValidationMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        IAntiforgeryMetadata? metadata = context.GetEndpoint()?.Metadata
            .GetMetadata<IAntiforgeryMetadata>();

        if (metadata is not { RequiresValidation: true } || !IsUnsafeMethod(context.Request.Method))
        {
            await next(context);
            return;
        }

        try
        {
            await antiforgery.ValidateRequestAsync(context);
        }
        catch (AntiforgeryValidationException exception)
        {
            logger.LogWarning(
                exception,
                "Antiforgery validation failed for {Path}",
                context.Request.Path);

            ProblemDetails problemDetails = ProblemDetailsResponse.Create(
                context,
                StatusCodes.Status400BadRequest,
                "antiforgery_failed",
                "Request could not be verified",
                "Reload the page and try again.");

            await ProblemDetailsResponse.WriteAsync(context, problemDetails);
            return;
        }

        await next(context);
    }

    private static bool IsUnsafeMethod(string method) =>
        HttpMethods.IsPost(method) || HttpMethods.IsPut(method) || HttpMethods.IsPatch(method);
}
