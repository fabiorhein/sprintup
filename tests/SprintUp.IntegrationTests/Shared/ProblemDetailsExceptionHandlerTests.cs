using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SprintUp.Shared.Domain;
using SprintUp.Shared.Infrastructure;

namespace SprintUp.IntegrationTests.Shared;

public sealed class ProblemDetailsExceptionHandlerTests
{
    private const string CorrelationId = "corr-7f3a9c";

    [Fact]
    public async Task DomainConflict_MapsTo409WithStableCode()
    {
        DomainException exception = new(
            "email_already_registered",
            "This email is already registered. Sign in or recover your password.");

        (DefaultHttpContext context, JsonDocument body) = await HandleAsync(exception);

        using (body)
        {
            context.Response.StatusCode.Should().Be(409);
            context.Response.ContentType.Should().StartWith("application/problem+json");
            context.Response.Headers["X-Correlation-ID"].ToString().Should().Be(CorrelationId);
            body.RootElement.GetProperty("code").GetString().Should().Be("email_already_registered");
            body.RootElement.GetProperty("detail").GetString()
                .Should().Be("This email is already registered. Sign in or recover your password.");
            body.RootElement.GetProperty("correlationId").GetString().Should().Be(CorrelationId);
        }
    }

    [Fact]
    public async Task Validation_MapsTo400WithPerFieldErrors()
    {
        Dictionary<string, string[]> errors = new()
        {
            ["email"] = ["Email is required."],
            ["weeklyAvailabilityHours"] = ["Availability must be between 0 and 40 hours."],
        };

        (DefaultHttpContext context, JsonDocument body) = await HandleAsync(new ValidationException(errors));

        using (body)
        {
            context.Response.StatusCode.Should().Be(400);
            body.RootElement.GetProperty("code").GetString().Should().Be("validation_failed");
            JsonElement fieldErrors = body.RootElement.GetProperty("errors");
            fieldErrors.GetProperty("email")[0].GetString().Should().Be("Email is required.");
            fieldErrors.GetProperty("weeklyAvailabilityHours")[0].GetString()
                .Should().Be("Availability must be between 0 and 40 hours.");
        }
    }

    [Fact]
    public async Task NotFound_MapsTo404WithStableCode()
    {
        (DefaultHttpContext context, JsonDocument body) = await HandleAsync(
            new NotFoundException("Activity was not found.", "activity_not_found"));

        using (body)
        {
            context.Response.StatusCode.Should().Be(404);
            body.RootElement.GetProperty("code").GetString().Should().Be("activity_not_found");
            body.RootElement.GetProperty("detail").GetString().Should().Be("Activity was not found.");
        }
    }

    [Fact]
    public async Task UnexpectedError_Returns500WithCorrelationId_AndNoInternals()
    {
        const string secret = "token=super-secret-value";
        const string sql = "select * from users";

        (DefaultHttpContext context, JsonDocument body) = await HandleAsync(
            new InvalidOperationException($"{secret} {sql}"));

        string rawBody = body.RootElement.GetRawText();
        using (body)
        {
            context.Response.StatusCode.Should().Be(500);
            context.Response.Headers["X-Correlation-ID"].ToString().Should().Be(CorrelationId);
            body.RootElement.GetProperty("code").GetString().Should().Be("internal_error");
            body.RootElement.GetProperty("correlationId").GetString().Should().Be(CorrelationId);
            rawBody.Should().NotContain(secret);
            rawBody.Should().NotContain(sql);
            rawBody.Should().NotContain("InvalidOperationException");
            rawBody.Should().NotContain("stackTrace");
        }
    }

    private static async Task<(DefaultHttpContext Context, JsonDocument Body)> HandleAsync(Exception exception)
    {
        ServiceCollection services = new();
        services.AddLogging();
        services.AddProblemDetails();

        await using ServiceProvider provider = services.BuildServiceProvider();
        ProblemDetailsExceptionHandler handler = new(
            provider.GetRequiredService<IProblemDetailsService>(),
            provider.GetRequiredService<ILogger<ProblemDetailsExceptionHandler>>());

        DefaultHttpContext context = new()
        {
            RequestServices = provider,
            TraceIdentifier = CorrelationId,
        };
        context.Request.Path = "/api/v1/probe";
        context.Response.Body = new MemoryStream();

        bool handled = await handler.TryHandleAsync(context, exception, CancellationToken.None);
        handled.Should().BeTrue();

        context.Response.Body.Position = 0;
        JsonDocument body = await JsonDocument.ParseAsync(context.Response.Body);
        return (context, body);
    }
}
