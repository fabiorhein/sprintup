using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SprintUp.IntegrationTests.Infrastructure;
using SprintUp.Shared.Infrastructure;

namespace SprintUp.IntegrationTests.Shared;

[Collection(PostgresCollection.Name)]
public sealed class SprintUpDbContextTests(PostgresFixture fixture)
{
    [Fact]
    public void Model_BuildsWithoutErrors()
    {
        using ProbeDbContext context = CreateContext();

        var entity = context.Model.FindEntityType(typeof(ProbeEntity));

        entity.Should().NotBeNull();
        entity!.FindProperty(nameof(ProbeEntity.Status))!.GetProviderClrType().Should().Be(typeof(int));
        entity.FindProperty(nameof(ProbeEntity.PayloadJson))!.GetColumnType().Should().Be("jsonb");
        entity.FindProperty(nameof(ProbeEntity.Metadata))!.GetColumnType().Should().Be("jsonb");
        entity.FindProperty(nameof(ProbeEntity.CreatedAt))!.GetColumnType().Should().Be("timestamptz");
        entity.FindProperty(nameof(ProbeEntity.NormalizedEmail))!.IsNullable.Should().BeFalse();
        entity.FindProperty("xmin")!.IsConcurrencyToken.Should().BeTrue();
        entity.GetIndexes().Should().Contain(
            index => index.Properties.Any(property => property.Name == nameof(ProbeEntity.NormalizedEmail)));
    }

    [Fact]
    public async Task CanPersistAndRollback()
    {
        await using ProbeDbContext context = CreateContext();
        await context.Database.EnsureCreatedAsync();

        ProbeEntity probe = new()
        {
            Id = Guid.NewGuid(),
            Email = "runner@example.com",
            NormalizedEmail = "runner@example.com",
            Status = ProbeStatus.Active,
            PayloadJson = "{\"source\":\"probe\"}",
            Metadata = "{}",
            CreatedAt = new DateTimeOffset(2026, 10, 27, 12, 0, 0, TimeSpan.FromHours(-3)),
        };

        await using var transaction = await context.Database.BeginTransactionAsync();
        context.Add(probe);
        await context.SaveChangesAsync();

        context.ChangeTracker.Clear();
        ProbeEntity persisted = await context.Set<ProbeEntity>().SingleAsync();
        persisted.CreatedAt.Offset.Should().Be(TimeSpan.Zero);
        persisted.CreatedAt.UtcDateTime.Should().Be(probe.CreatedAt.UtcDateTime);

        (await context.Set<ProbeEntity>().CountAsync()).Should().Be(1);

        await transaction.RollbackAsync();

        (await context.Set<ProbeEntity>().CountAsync()).Should().Be(0);
    }

    private ProbeDbContext CreateContext()
    {
        DbContextOptions<SprintUpDbContext> options = new DbContextOptionsBuilder<SprintUpDbContext>()
            .UseNpgsql(fixture.ConnectionString)
            .Options;

        return new ProbeDbContext(options);
    }

    private sealed class ProbeDbContext(DbContextOptions<SprintUpDbContext> options) : SprintUpDbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ProbeEntity>();
            base.OnModelCreating(modelBuilder);
        }
    }

    private sealed class ProbeEntity : IConcurrencyChecked
    {
        public Guid Id { get; set; }

        public string Email { get; set; } = string.Empty;

        public string NormalizedEmail { get; set; } = string.Empty;

        public ProbeStatus Status { get; set; }

        public string PayloadJson { get; set; } = "{}";

        [Jsonb]
        public string Metadata { get; set; } = "{}";

        public DateTimeOffset CreatedAt { get; set; }
    }

    private enum ProbeStatus
    {
        Pending,
        Active,
    }
}
