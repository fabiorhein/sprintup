using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace SprintUp.IntegrationTests.Host;

/// <summary>
/// Starts the real host in the Testing environment. Tests provide the fixture connection string
/// and a throwaway persistent key-ring directory; the production appsettings stay untouched.
/// </summary>
internal sealed class SprintUpWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string connectionString;
    private readonly string? dataProtectionPath;
    private readonly IReadOnlyDictionary<string, string?> overrides;

    public SprintUpWebApplicationFactory(
        string connectionString,
        string? dataProtectionPath,
        IReadOnlyDictionary<string, string?>? overrides = null)
    {
        this.connectionString = connectionString;
        this.dataProtectionPath = dataProtectionPath;
        this.overrides = overrides ?? new Dictionary<string, string?>();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        Dictionary<string, string?> settings = new()
        {
            ["ConnectionStrings:SprintUp"] = connectionString,
            ["DataProtection:KeysPath"] = dataProtectionPath ?? string.Empty,
            ["Diagnostics:Enabled"] = "true",
        };

        foreach (KeyValuePair<string, string?> setting in overrides)
        {
            settings[setting.Key] = setting.Value;
        }

        foreach (KeyValuePair<string, string?> setting in settings)
        {
            builder.UseSetting(setting.Key, setting.Value ?? string.Empty);
        }

        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            configuration.AddInMemoryCollection(settings);
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing && !string.IsNullOrWhiteSpace(dataProtectionPath) &&
            Directory.Exists(dataProtectionPath))
        {
            Directory.Delete(dataProtectionPath, recursive: true);
        }
    }
}
