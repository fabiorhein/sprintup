using Microsoft.AspNetCore.DataProtection;

namespace SprintUp.Shared.Infrastructure;

/// <summary>
/// Configures the application key ring. Outside Development a configured, persistent directory
/// is mandatory so Strava tokens stay readable across restarts and deployments.
/// </summary>
public static class DataProtectionRegistration
{
    public const string ApplicationName = "SprintUp";

    public static IServiceCollection AddSprintUpDataProtection(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        IDataProtectionBuilder dataProtection = services
            .AddDataProtection()
            .SetApplicationName(ApplicationName);

        string? configuredPath = configuration["DataProtection:KeysPath"];
        if (string.IsNullOrWhiteSpace(configuredPath))
        {
            if (!environment.IsDevelopment())
            {
                throw new InvalidOperationException(
                    "DataProtection:KeysPath must be configured outside Development. " +
                    "Set DataProtection__KeysPath to a writable persistent directory so stored tokens " +
                    "remain readable across restarts.");
            }

            return services;
        }

        string fullPath = Path.GetFullPath(configuredPath);
        try
        {
            Directory.CreateDirectory(fullPath);
        }
        catch (Exception exception)
        {
            throw new InvalidOperationException(
                $"DataProtection:KeysPath '{fullPath}' must be a writable directory.",
                exception);
        }

        dataProtection.PersistKeysToFileSystem(new DirectoryInfo(fullPath));
        return services;
    }
}
