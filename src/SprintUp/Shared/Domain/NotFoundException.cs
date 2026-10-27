namespace SprintUp.Shared.Domain;

/// <summary>
/// A requested resource does not exist or is not visible to the caller.
/// </summary>
public sealed class NotFoundException : Exception
{
    public NotFoundException(string message, string code = "not_found")
        : base(message)
    {
        Code = code;
    }

    public string Code { get; }
}
