using Microsoft.AspNetCore.Mvc;

namespace SprintUp.Shared.Infrastructure;

/// <summary>
/// Creates and writes the RFC 7231 error shape shared by exception handling, antiforgery
/// failures, and rate-limit rejections. Stable codes let the UI localize a message without
/// relying on server text.
/// </summary>
internal static class ProblemDetailsResponse
{
    public const string CorrelationHeader = "X-Correlation-ID";

    public static ProblemDetails Create(
        HttpContext context,
        int status,
        string code,
        string title,
        string? detail = null)
    {
        ProblemDetails problemDetails = new()
        {
            Status = status,
            Title = title,
            Detail = detail,
            Instance = context.Request.Path,
            Type = $"https://httpstatuses.io/{status}",
        };

        problemDetails.Extensions["code"] = code;
        problemDetails.Extensions["correlationId"] = context.TraceIdentifier;
        return problemDetails;
    }

    public static async ValueTask WriteAsync(HttpContext context, ProblemDetails problemDetails)
    {
        context.Response.StatusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError;
        context.Response.Headers[CorrelationHeader] = context.TraceIdentifier;

        IProblemDetailsService problemDetailsService =
            context.RequestServices.GetRequiredService<IProblemDetailsService>();

        await problemDetailsService.WriteAsync(new ProblemDetailsContext
        {
            HttpContext = context,
            ProblemDetails = problemDetails,
        });
    }
}
