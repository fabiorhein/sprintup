using System.Globalization;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Localization;
using SprintUp.Components;
using SprintUp.Shared.Infrastructure;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

string? connectionString = builder.Configuration.GetConnectionString("SprintUp");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "ConnectionStrings:SprintUp is required. Set ConnectionStrings__SprintUp to a PostgreSQL 16 connection string.");
}

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddSprintUpModules(connectionString);
builder.Services.AddSprintUpDataProtection(builder.Configuration, builder.Environment);
builder.Services.AddSprintUpSecurity(builder.Environment);
builder.Services.AddSprintUpRateLimiting(builder.Configuration);
builder.Services.AddSprintUpWorkers();

builder.Services.AddLocalization();
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    CultureInfo[] supportedCultures =
    [
        CultureInfo.GetCultureInfo("pt-BR"),
        CultureInfo.GetCultureInfo("en"),
    ];

    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
    options.DefaultRequestCulture = new RequestCulture("pt-BR", "pt-BR");
    options.RequestCultureProviders =
    [
        new CookieRequestCultureProvider(),
        new AcceptLanguageHeaderRequestCultureProvider(),
    ];
    options.ApplyCurrentCultureToResponseHeaders = true;
});

builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";
    options.Cookie.Name = "SprintUp.Antiforgery";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.SecurePolicy = IsLocalEnvironment(builder.Environment)
        ? CookieSecurePolicy.SameAsRequest
        : CookieSecurePolicy.Always;
});

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ProblemDetailsExceptionHandler>();
builder.Services.AddHealthChecks()
    .AddCheck<DatabaseHealthCheck>("database", tags: [HealthCheckTags.Ready]);

WebApplication app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseRouting();
app.UseRequestLocalization();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<AntiforgeryValidationMiddleware>();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapHealthChecks("/health", new HealthCheckOptions
{
    Predicate = registration => registration.Tags.Contains(HealthCheckTags.Ready),
    ResponseWriter = static (context, report) =>
    {
        context.Response.ContentType = "application/json; charset=utf-8";
        return context.Response.WriteAsJsonAsync(new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(entry => new
            {
                name = entry.Key,
                status = entry.Value.Status.ToString(),
            }),
        });
    },
});

if (app.Environment.IsEnvironment("Testing") &&
    app.Configuration.GetValue<bool>("Diagnostics:Enabled"))
{
    app.MapSprintUpDiagnostics();
}

app.Run();

static bool IsLocalEnvironment(IHostEnvironment environment) =>
    environment.IsDevelopment() || environment.IsEnvironment("Testing");

public partial class Program;
