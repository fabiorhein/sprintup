using System.Globalization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace SprintUp.Shared.Infrastructure;

/// <summary>
/// Names the rate-limit policies used by state-changing auth endpoints.
/// </summary>
public static class RateLimitingPolicies
{
    public const string Auth = "auth";
}

/// <summary>
/// Registers fixed-window rate limiting. Auth endpoints opt in with
/// <c>RequireRateLimiting(RateLimitingPolicies.Auth)</c>; the partition is per client IP until
/// authenticated actions can add a normalized-email partition.
/// </summary>
public static class RateLimitingRegistration
{
    private const int DefaultPermitLimit = 10;
    private const int DefaultWindowSeconds = 60;

    public static IServiceCollection AddSprintUpRateLimiting(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        int permitLimit = configuration.GetValue<int?>("RateLimiting:Auth:PermitLimit")
            ?? DefaultPermitLimit;
        int windowSeconds = configuration.GetValue<int?>("RateLimiting:Auth:WindowSeconds")
            ?? DefaultWindowSeconds;

        if (permitLimit < 1)
        {
            throw new InvalidOperationException("RateLimiting:Auth:PermitLimit must be at least 1.");
        }

        if (windowSeconds < 1)
        {
            throw new InvalidOperationException("RateLimiting:Auth:WindowSeconds must be at least 1.");
        }

        TimeSpan window = TimeSpan.FromSeconds(windowSeconds);

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.AddPolicy(RateLimitingPolicies.Auth, httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = permitLimit,
                        Window = window,
                        QueueLimit = 0,
                        AutoReplenishment = true,
                    }));

            options.OnRejected = WriteRejectionAsync;
        });

        return services;
    }

    private static async ValueTask WriteRejectionAsync(
        OnRejectedContext context,
        CancellationToken cancellationToken)
    {
        HttpContext httpContext = context.HttpContext;
        if (httpContext.Response.HasStarted)
        {
            return;
        }

        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out TimeSpan retryAfter))
        {
            httpContext.Response.Headers.RetryAfter = Math
                .Ceiling(retryAfter.TotalSeconds)
                .ToString(CultureInfo.InvariantCulture);
        }

        ProblemDetails problemDetails = ProblemDetailsResponse.Create(
            httpContext,
            StatusCodes.Status429TooManyRequests,
            "rate_limited",
            "Too many requests",
            "Wait before trying again.");

        await ProblemDetailsResponse.WriteAsync(httpContext, problemDetails);
    }
}
