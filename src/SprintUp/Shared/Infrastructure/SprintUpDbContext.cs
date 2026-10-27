using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace SprintUp.Shared.Infrastructure;

/// <summary>
/// The single database context for the modular monolith. Modules declare their entities and
/// configurations inside their own folders; this type only owns the shared conventions.
/// </summary>
public class SprintUpDbContext : DbContext
{
    // Module-owned DbSets are declared here as their aggregates land:
    // UserManagement (T9-T13), Coaching (T15-T18), Integrations (T46-T48),
    // Training (T42-T43), and Subscriptions (T40, T68).

    public SprintUpDbContext(DbContextOptions<SprintUpDbContext> options)
        : base(options)
    {
    }

    protected SprintUpDbContext(DbContextOptions options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SprintUpDbContext).Assembly);
        ApplyConventions(modelBuilder);
    }

    private static void ApplyConventions(ModelBuilder modelBuilder)
    {
        foreach (IMutableEntityType entityType in modelBuilder.Model.GetEntityTypes())
        {
            EntityTypeBuilder entity = modelBuilder.Entity(entityType.ClrType);

            foreach (IMutableProperty property in entityType.GetProperties())
            {
                ApplyPropertyConventions(entity, property);
            }

            ApplyNormalizedEmailConventions(entity, entityType);
            ApplyConcurrencyConvention(entity, entityType);
        }
    }

    private static void ApplyPropertyConventions(EntityTypeBuilder entity, IMutableProperty property)
    {
        Type clrType = Nullable.GetUnderlyingType(property.ClrType) ?? property.ClrType;

        if (clrType.IsEnum && property.GetProviderClrType() is null)
        {
            property.SetProviderClrType(typeof(int));
            return;
        }

        if (clrType == typeof(DateTimeOffset) || clrType == typeof(DateTime))
        {
            property.SetColumnType("timestamptz");
            if (property.GetValueConverter() is null)
            {
                property.SetValueConverter(CreateUtcConverter(property.ClrType));
            }

            return;
        }

        if (clrType == typeof(string) && IsJsonProperty(property))
        {
            property.SetColumnType("jsonb");
            return;
        }

        if (clrType == typeof(string) && IsNormalizedEmailProperty(property))
        {
            property.SetMaxLength(320);
            property.IsNullable = false;
            entity.HasIndex(property.Name);
        }
    }

    private static void ApplyNormalizedEmailConventions(EntityTypeBuilder entity, IMutableEntityType entityType)
    {
        foreach (IMutableProperty property in entityType.GetProperties())
        {
            if (property.ClrType != typeof(string))
            {
                continue;
            }

            if (IsEmailProperty(property) || IsNormalizedEmailProperty(property))
            {
                property.SetMaxLength(320);
            }

            if (IsEmailProperty(property) && property.IsNullable)
            {
                property.IsNullable = false;
            }
        }
    }

    private static void ApplyConcurrencyConvention(EntityTypeBuilder entity, IMutableEntityType entityType)
    {
        if (!typeof(IConcurrencyChecked).IsAssignableFrom(entityType.ClrType))
        {
            return;
        }

        IMutableProperty xmin = entity.Property<uint>("xmin").Metadata;
        xmin.SetColumnName("xmin");
        xmin.SetColumnType("xid");
        xmin.ValueGenerated = ValueGenerated.OnAddOrUpdate;
        xmin.IsConcurrencyToken = true;
    }

    private static ValueConverter CreateUtcConverter(Type propertyType)
    {
        if (propertyType == typeof(DateTimeOffset))
        {
            return new ValueConverter<DateTimeOffset, DateTimeOffset>(
                value => value.ToUniversalTime(),
                value => value.ToUniversalTime());
        }

        if (propertyType == typeof(DateTimeOffset?))
        {
            return new ValueConverter<DateTimeOffset?, DateTimeOffset?>(
                value => value.HasValue ? value.Value.ToUniversalTime() : null,
                value => value.HasValue ? value.Value.ToUniversalTime() : null);
        }

        if (propertyType == typeof(DateTime?))
        {
            return new ValueConverter<DateTime?, DateTime?>(
                value => value.HasValue ? ToUtc(value.Value) : null,
                value => value.HasValue ? DateTime.SpecifyKind(value.Value, DateTimeKind.Utc) : null);
        }

        return new ValueConverter<DateTime, DateTime>(
            value => ToUtc(value),
            value => DateTime.SpecifyKind(value, DateTimeKind.Utc));
    }

    private static DateTime ToUtc(DateTime value) =>
        value.Kind == DateTimeKind.Utc ? value : value.ToUniversalTime();

    private static bool IsJsonProperty(IReadOnlyProperty property)
    {
        if (property.PropertyInfo?.GetCustomAttribute<JsonbAttribute>() is not null)
        {
            return true;
        }

        return property.Name.EndsWith("Json", StringComparison.Ordinal) ||
               string.Equals(property.Name, "RawPayload", StringComparison.Ordinal);
    }

    private static bool IsNormalizedEmailProperty(IReadOnlyProperty property) =>
        property.Name.StartsWith("Normalized", StringComparison.Ordinal) &&
        property.Name.EndsWith("Email", StringComparison.Ordinal);

    private static bool IsEmailProperty(IReadOnlyProperty property) =>
        property.Name.EndsWith("Email", StringComparison.Ordinal) &&
        !IsNormalizedEmailProperty(property);
}
