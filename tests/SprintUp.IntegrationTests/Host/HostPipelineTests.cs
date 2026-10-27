using System.Net;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using SprintUp.IntegrationTests.Infrastructure;
using SprintUp.Shared.Infrastructure;

namespace SprintUp.IntegrationTests.Host;

[Collection(PostgresCollection.Name)]
public sealed class HostPipelineTests(PostgresFixture fixture)
{
    private static readonly string[] Policies =
    [
        SprintUpPolicies.AthleteOnly,
        SprintUpPolicies.ActiveAthlete,
        SprintUpPolicies.AssignedCoach,
        SprintUpPolicies.CoachReadOnly,
        SprintUpPolicies.AdminOnly,
    ];

    [Fact]
    public async Task Host_ReportsHealthyDatabase_AndRegistersPlaceholderPolicies()
    {
        await using SprintUpWebApplicationFactory factory = CreateFactory();

        using HttpClient client = factory.CreateClient();
        using HttpResponseMessage response = await client.GetAsync("/health");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        using JsonDocument body = await JsonDocument.ParseAsync(
            await response.Content.ReadAsStreamAsync());
        body.RootElement.GetProperty("status").GetString().Should().Be("Healthy");

        JsonElement database = body.RootElement.GetProperty("checks")
            .EnumerateArray()
            .Single(check => check.GetProperty("name").GetString() == "database");
        database.GetProperty("status").GetString().Should().Be("Healthy");

        IAuthorizationPolicyProvider policyProvider =
            factory.Services.GetRequiredService<IAuthorizationPolicyProvider>();
        foreach (string policy in Policies)
        {
            (await policyProvider.GetPolicyAsync(policy)).Should().NotBeNull(
                $"the placeholder policy {policy} must be registered");
        }

        IAuthenticationSchemeProvider schemeProvider =
            factory.Services.GetRequiredService<IAuthenticationSchemeProvider>();
        (await schemeProvider.GetSchemeAsync(SprintUpAuthentication.Scheme)).Should().NotBeNull();
    }

    [Fact]
    public async Task Requests_DefaultToPtBr_AndServeEnglish()
    {
        await using SprintUpWebApplicationFactory factory = CreateFactory();
        using HttpClient client = factory.CreateClient();

        using HttpResponseMessage defaultResponse = await client.GetAsync("/");
        defaultResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        defaultResponse.Content.Headers.ContentLanguage.Should().ContainSingle("pt-BR");
        (await defaultResponse.Content.ReadAsStringAsync())
            .Should().Contain("<html lang=\"pt-BR\"");

        using HttpRequestMessage englishRequest = new(HttpMethod.Get, "/");
        englishRequest.Headers.AcceptLanguage.ParseAdd("en-US,en;q=0.9");
        using HttpResponseMessage englishResponse = await client.SendAsync(englishRequest);

        englishResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        englishResponse.Content.Headers.ContentLanguage.Should().ContainSingle("en");
        (await englishResponse.Content.ReadAsStringAsync())
            .Should().Contain("<html lang=\"en\"");
    }

    [Fact]
    public async Task AntiforgeryFailure_ReturnsProblemDetails()
    {
        await using SprintUpWebApplicationFactory factory = CreateFactory();
        using HttpClient client = factory.CreateClient();

        using HttpResponseMessage response = await client.PostAsync(
            "/api/v1/_diagnostics/antiforgery",
            new StringContent("{}", System.Text.Encoding.UTF8, "application/json"));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.Content.Headers.ContentType!.MediaType.Should().Be("application/problem+json");
        response.Headers.GetValues("X-Correlation-ID").Should().ContainSingle();

        using JsonDocument body = await JsonDocument.ParseAsync(
            await response.Content.ReadAsStreamAsync());
        body.RootElement.GetProperty("code").GetString().Should().Be("antiforgery_failed");
        body.RootElement.GetProperty("correlationId").GetString()
            .Should().Be(response.Headers.GetValues("X-Correlation-ID").Single());
    }

    [Fact]
    public async Task RateLimit_Returns429WithRetryAfter()
    {
        await using SprintUpWebApplicationFactory factory = CreateFactory(new Dictionary<string, string?>
        {
            ["RateLimiting:Auth:PermitLimit"] = "2",
            ["RateLimiting:Auth:WindowSeconds"] = "60",
        });
        using HttpClient client = factory.CreateClient();

        (await client.GetAsync("/api/v1/_diagnostics/rate-limit")).StatusCode
            .Should().Be(HttpStatusCode.NoContent);
        (await client.GetAsync("/api/v1/_diagnostics/rate-limit")).StatusCode
            .Should().Be(HttpStatusCode.NoContent);

        using HttpResponseMessage response =
            await client.GetAsync("/api/v1/_diagnostics/rate-limit");

        response.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
        response.Headers.RetryAfter.Should().NotBeNull();
        response.Headers.RetryAfter!.Delta.Should().BeGreaterThan(TimeSpan.Zero);
        response.Content.Headers.ContentType!.MediaType.Should().Be("application/problem+json");

        using JsonDocument body = await JsonDocument.ParseAsync(
            await response.Content.ReadAsStreamAsync());
        body.RootElement.GetProperty("code").GetString().Should().Be("rate_limited");
    }

    [Fact]
    public async Task NonDevelopmentHost_RefusesEphemeralDataProtection_AndMapsUnexpectedErrors()
    {
        await using (SprintUpWebApplicationFactory refused =
            new(fixture.ConnectionString, dataProtectionPath: null))
        {
            Action createClient = () => refused.CreateClient();

            createClient.Should().Throw<InvalidOperationException>()
                .WithMessage("*DataProtection:KeysPath*");
        }

        await using SprintUpWebApplicationFactory factory = CreateFactory();
        using HttpClient client = factory.CreateClient();

        using HttpResponseMessage response =
            await client.GetAsync("/api/v1/_diagnostics/errors/unhandled");

        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        response.Content.Headers.ContentType!.MediaType.Should().Be("application/problem+json");
        response.Headers.GetValues("X-Correlation-ID").Should().ContainSingle();

        string rawBody = await response.Content.ReadAsStringAsync();
        using JsonDocument body = JsonDocument.Parse(rawBody);

        body.RootElement.GetProperty("code").GetString().Should().Be("internal_error");
        body.RootElement.GetProperty("correlationId").GetString()
            .Should().Be(response.Headers.GetValues("X-Correlation-ID").Single());
        rawBody.Should().NotContain("super-secret-value");
        rawBody.Should().NotContain("select * from users");
        rawBody.Should().NotContain("InvalidOperationException");
        rawBody.Should().NotContain("stackTrace");
    }

    private SprintUpWebApplicationFactory CreateFactory(
        IReadOnlyDictionary<string, string?>? overrides = null)
    {
        string keysPath = Path.Combine(
            Path.GetTempPath(),
            "opencode",
            "sprintup-tests",
            Guid.NewGuid().ToString("N"));

        return new SprintUpWebApplicationFactory(
            fixture.ConnectionString,
            keysPath,
            overrides);
    }
}
