namespace SprintUp.Shared.Infrastructure;

/// <summary>
/// Marks an entity whose persistence rows use PostgreSQL <c>xmin</c> for optimistic concurrency.
/// </summary>
public interface IConcurrencyChecked;

/// <summary>
/// Marks a string property stored as PostgreSQL <c>jsonb</c>. Properties whose name ends in
/// <c>Json</c> or is <c>RawPayload</c> are also treated as JSON by convention.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class JsonbAttribute : Attribute;
