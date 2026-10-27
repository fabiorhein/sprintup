using System.Globalization;
using Microsoft.AspNetCore.Antiforgery;

namespace SprintUp.Shared.Infrastructure;

/// <summary>
/// Testing-environment probes for the host pipeline. They are never mapped in Development,
/// Production, or Staging, and they exist so integration tests can exercise the real middleware
/// instead of reimplementing it.
/// </summary>
internal static class DiagnosticsEndpoints
{
    public static IEndpointRouteBuilder MapSprintUpDiagnostics(this IEndpointRouteBuilder endpoints)
    {
        RouteGroupBuilder group = endpoints.MapGroup("/api/v1/_diagnostics");

        group.MapGet("/culture", () => Results.Ok(new
        {
            culture = CultureInfo.CurrentCulture.Name,
            uiCulture = CultureInfo.CurrentUICulture.Name,
        }));

        group.MapPost("/antiforgery", () => Results.NoContent())
            .WithMetadata(new RequireAntiforgeryTokenAttribute(required: true));

        group.MapGet("/rate-limit", () => Results.NoContent())
            .RequireRateLimiting(RateLimitingPolicies.Auth);

        group.MapGet("/errors/unhandled", UnhandledError);

        return endpoints;
    }

    private static IResult UnhandledError() =>
        throw new InvalidOperationException(
            "Probe failure: token=super-secret-value select * from users");
}
