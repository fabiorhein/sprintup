namespace SprintUp.Shared.Infrastructure;

/// <summary>
/// Names the cookie scheme used by every authenticated request.
/// </summary>
public static class SprintUpAuthentication
{
    public const string Scheme = "SprintUp.Auth";
}

/// <summary>
/// Role names issued on the authentication cookie.
/// </summary>
public static class SprintUpRoles
{
    public const string Athlete = "Athlete";
    public const string Coach = "Coach";
    public const string Admin = "Admin";
}

/// <summary>
/// Claim names shared by policies and the account module.
/// </summary>
public static class SprintUpClaimTypes
{
    public const string AccountStatus = "account_status";
}

/// <summary>
/// Authorization policy names consumed by later modules.
/// </summary>
public static class SprintUpPolicies
{
    public const string AthleteOnly = "AthleteOnly";
    public const string ActiveAthlete = "ActiveAthlete";
    public const string AssignedCoach = "AssignedCoach";
    public const string CoachReadOnly = "CoachReadOnly";
    public const string AdminOnly = "AdminOnly";
}

/// <summary>
/// Registers the cookie scheme and placeholder policies. Assignment, entitlement, and read-only
/// checks are completed by their owning modules; the names stay stable for route authorization.
/// </summary>
public static class SecurityRegistration
{
    public static IServiceCollection AddSprintUpSecurity(
        this IServiceCollection services,
        IHostEnvironment environment)
    {
        services.AddAuthentication(SprintUpAuthentication.Scheme)
            .AddCookie(SprintUpAuthentication.Scheme, options =>
            {
                options.Cookie.Name = "SprintUp.Auth";
                options.Cookie.HttpOnly = true;
                options.Cookie.SameSite = SameSiteMode.Lax;
                options.Cookie.SecurePolicy = IsLocalEnvironment(environment)
                    ? CookieSecurePolicy.SameAsRequest
                    : CookieSecurePolicy.Always;
                options.ExpireTimeSpan = TimeSpan.FromDays(14);
                options.SlidingExpiration = true;
                options.LoginPath = "/login";
                options.AccessDeniedPath = "/login";
                options.ReturnUrlParameter = "returnUrl";
            });

        services.AddAuthorization(options =>
        {
            options.AddPolicy(SprintUpPolicies.AthleteOnly, policy => policy
                .RequireAuthenticatedUser()
                .RequireRole(SprintUpRoles.Athlete));

            options.AddPolicy(SprintUpPolicies.ActiveAthlete, policy => policy
                .RequireAuthenticatedUser()
                .RequireRole(SprintUpRoles.Athlete)
                .RequireClaim(SprintUpClaimTypes.AccountStatus, "Active"));

            options.AddPolicy(SprintUpPolicies.AssignedCoach, policy => policy
                .RequireAuthenticatedUser()
                .RequireRole(SprintUpRoles.Coach));

            options.AddPolicy(SprintUpPolicies.CoachReadOnly, policy => policy
                .RequireAuthenticatedUser()
                .RequireRole(SprintUpRoles.Coach));

            options.AddPolicy(SprintUpPolicies.AdminOnly, policy => policy
                .RequireAuthenticatedUser()
                .RequireRole(SprintUpRoles.Admin));
        });

        return services;
    }

    private static bool IsLocalEnvironment(IHostEnvironment environment) =>
        environment.IsDevelopment() || environment.IsEnvironment("Testing");
}
