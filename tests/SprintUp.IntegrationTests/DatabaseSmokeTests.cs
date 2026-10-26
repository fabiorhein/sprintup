using FluentAssertions;
using Npgsql;
using SprintUp.IntegrationTests.Infrastructure;

namespace SprintUp.IntegrationTests;

[Collection(PostgresCollection.Name)]
public sealed class DatabaseSmokeTests(PostgresFixture fixture)
{
    [Fact]
    public async Task CanConnectAndCreateSchema()
    {
        await using NpgsqlConnection connection = new(fixture.ConnectionString);
        await connection.OpenAsync();

        await using (NpgsqlCommand create = connection.CreateCommand())
        {
            create.CommandText =
                "create table smoke_check (id integer primary key, created_at timestamptz not null default now())";
            await create.ExecuteNonQueryAsync();
        }

        await using (NpgsqlCommand insert = connection.CreateCommand())
        {
            insert.CommandText = "insert into smoke_check (id) values (1)";
            await insert.ExecuteNonQueryAsync();
        }

        await using (NpgsqlCommand select = connection.CreateCommand())
        {
            select.CommandText = "select current_schema(), count(*) from smoke_check";
            await using NpgsqlDataReader reader = await select.ExecuteReaderAsync();
            (await reader.ReadAsync()).Should().BeTrue();
            reader.GetString(0).Should().Be(fixture.Schema);
            reader.GetInt64(1).Should().Be(1);
        }
    }
}
