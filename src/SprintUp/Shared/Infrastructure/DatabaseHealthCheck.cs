using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace SprintUp.Shared.Infrastructure;

/// <summary>
/// Readiness check for PostgreSQL. It opens a connection through the application context and
/// reports only a status, never the connection string or provider error text.
/// </summary>
public sealed class DatabaseHealthCheck(SprintUpDbContext dbContext) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            bool canConnect = await dbContext.Database.CanConnectAsync(cancellationToken);
            return canConnect
                ? HealthCheckResult.Healthy("Database is reachable.")
                : HealthCheckResult.Unhealthy("Database is not reachable.");
        }
        catch (Exception)
        {
            return HealthCheckResult.Unhealthy("Database is not reachable.");
        }
    }
}

/// <summary>
/// Health-check tags used by the host. Only the <c>ready</c> tag is exposed on <c>/health</c>.
/// </summary>
public static class HealthCheckTags
{
    public const string Ready = "ready";
}
