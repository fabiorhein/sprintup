using Npgsql;
using Testcontainers.PostgreSql;

namespace SprintUp.IntegrationTests.Infrastructure;

/// <summary>
/// Provides an isolated PostgreSQL schema for one test run. The schema is created on
/// initialization and dropped on disposal, so the shared database stays empty.
/// </summary>
public sealed class PostgresFixture : IAsyncLifetime
{
    private PostgreSqlContainer? container;
    private string adminConnectionString = string.Empty;

    public string Schema { get; private set; } = string.Empty;

    public string ConnectionString { get; private set; } = string.Empty;

    public async Task InitializeAsync()
    {
        adminConnectionString = await ResolveAdminConnectionStringAsync();

        await using NpgsqlConnection connection = new(adminConnectionString);
        await connection.OpenAsync();

        await RefuseNonEmptyDatabaseAsync(connection);

        Schema = $"test_{Guid.NewGuid():N}";
        await using (NpgsqlCommand create = connection.CreateCommand())
        {
            create.CommandText = $"CREATE SCHEMA \"{Schema}\"";
            await create.ExecuteNonQueryAsync();
        }

        NpgsqlConnectionStringBuilder builder = new(adminConnectionString)
        {
            SearchPath = Schema,
        };
        ConnectionString = builder.ConnectionString;
    }

    public async Task DisposeAsync()
    {
        if (!string.IsNullOrEmpty(Schema) && !string.IsNullOrEmpty(adminConnectionString))
        {
            await using NpgsqlConnection connection = new(adminConnectionString);
            await connection.OpenAsync();
            await using NpgsqlCommand drop = connection.CreateCommand();
            drop.CommandText = $"DROP SCHEMA IF EXISTS \"{Schema}\" CASCADE";
            await drop.ExecuteNonQueryAsync();
        }

        if (container is not null)
        {
            await container.DisposeAsync();
        }
    }

    private async Task<string> ResolveAdminConnectionStringAsync()
    {
        string? configured = Environment.GetEnvironmentVariable("TEST_DB_CONNECTION");
        if (!string.IsNullOrWhiteSpace(configured))
        {
            return configured;
        }

        try
        {
            container = new PostgreSqlBuilder("postgres:16")
                .WithDatabase("sprintup_test")
                .WithUsername("sprintup")
                .WithPassword("sprintup")
                .Build();
            await container.StartAsync();
            return container.GetConnectionString();
        }
        catch (Exception exception)
        {
            throw new InvalidOperationException(
                "PostgreSQL 16 is required for integration tests. Start Docker and let Testcontainers run, " +
                "or set TEST_DB_CONNECTION to an empty disposable PostgreSQL database.",
                exception);
        }
    }

    private static async Task RefuseNonEmptyDatabaseAsync(NpgsqlConnection connection)
    {
        await using (NpgsqlCommand tables = connection.CreateCommand())
        {
            tables.CommandText =
                "select count(*) from information_schema.tables " +
                "where table_schema = 'public' and table_type = 'BASE TABLE'";
            long count = (long)(await tables.ExecuteScalarAsync() ?? 0L);
            if (count > 0)
            {
                throw new InvalidOperationException(
                    "Integration tests need an empty database. The public schema already contains tables; " +
                    "drop them or point TEST_DB_CONNECTION at a disposable database.");
            }
        }

        await using NpgsqlCommand schemas = connection.CreateCommand();
        schemas.CommandText =
            "select nspname from pg_namespace " +
            "where nspname not like 'pg\\_%' and nspname not in ('information_schema', 'public')";
        await using NpgsqlDataReader reader = await schemas.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            string schema = reader.GetString(0);
            throw new InvalidOperationException(
                $"Integration tests need an empty database. Schema '{schema}' already exists; " +
                "drop it or point TEST_DB_CONNECTION at a disposable database.");
        }
    }
}
