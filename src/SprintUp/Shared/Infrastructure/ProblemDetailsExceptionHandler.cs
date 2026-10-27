using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SprintUp.Shared.Domain;

namespace SprintUp.Shared.Infrastructure;

/// <summary>
/// Maps known failures to RFC 7231 <see cref="ProblemDetails"/> with a stable code. Unexpected
/// failures return a correlation ID and never expose exception details, SQL, tokens, or health data.
/// </summary>
public sealed class ProblemDetailsExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<ProblemDetailsExceptionHandler> logger) : IExceptionHandler
{
    private const string InternalErrorCode = "internal_error";
    private const string CorrelationHeader = "X-Correlation-ID";

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        ProblemDetails problemDetails = exception switch
        {
            DomainException domain => Create(
                httpContext,
                domain.StatusCode,
                domain.Code,
                "Request could not be completed",
                domain.Message),
            ValidationException validation => CreateValidation(httpContext, validation),
            NotFoundException notFound => Create(
                httpContext,
                404,
                notFound.Code,
                "Resource not found",
                notFound.Message),
            _ => Create(
                httpContext,
                500,
                InternalErrorCode,
                "An unexpected error occurred",
                "Contact support with the correlation ID."),
        };

        if (problemDetails.Status == 500)
        {
            logger.LogError(
                exception,
                "Unhandled exception. CorrelationId {CorrelationId}",
                httpContext.TraceIdentifier);
        }

        httpContext.Response.StatusCode = problemDetails.Status!.Value;
        httpContext.Response.Headers[CorrelationHeader] = httpContext.TraceIdentifier;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problemDetails,
            Exception = exception,
        });
    }

    private static ProblemDetails CreateValidation(HttpContext httpContext, ValidationException exception)
    {
        ProblemDetails problemDetails = Create(
            httpContext,
            400,
            exception.Code,
            "Validation failed",
            "One or more input fields are invalid.");

        problemDetails.Extensions["errors"] = exception.Errors;
        return problemDetails;
    }

    private static ProblemDetails Create(
        HttpContext httpContext,
        int status,
        string code,
        string title,
        string? detail)
    {
        ProblemDetails problemDetails = new()
        {
            Status = status,
            Title = title,
            Detail = detail,
            Instance = httpContext.Request.Path,
            Type = $"https://httpstatuses.io/{status}",
        };

        problemDetails.Extensions["code"] = code;
        problemDetails.Extensions["correlationId"] = httpContext.TraceIdentifier;
        return problemDetails;
    }
}
