using Microsoft.EntityFrameworkCore;

namespace SprintUp.Shared.Infrastructure;

/// <summary>
/// Registers the shared persistence and module services. Each module extends this seam with its
/// own registrations; the host calls it once.
/// </summary>
public static class ModuleRegistration
{
    public static IServiceCollection AddSprintUpModules(
        this IServiceCollection services,
        string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        services.AddDbContext<SprintUpDbContext>(options => options.UseNpgsql(connectionString));

        return services;
    }
}
