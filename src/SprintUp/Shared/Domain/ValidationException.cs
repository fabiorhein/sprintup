namespace SprintUp.Shared.Domain;

/// <summary>
/// One or more input fields failed validation. Errors are keyed by field name.
/// </summary>
public sealed class ValidationException : Exception
{
    public ValidationException(IReadOnlyDictionary<string, string[]> errors, string code = "validation_failed")
        : base("One or more validation errors occurred.")
    {
        Errors = errors;
        Code = code;
    }

    public string Code { get; }

    public IReadOnlyDictionary<string, string[]> Errors { get; }
}
