namespace SprintUp.Shared.Domain;

/// <summary>
/// A rule violation with a stable error code that clients can localize.
/// </summary>
public class DomainException : Exception
{
    public DomainException(string code, string message, int statusCode = 409)
        : base(message)
    {
        Code = code;
        StatusCode = statusCode;
    }

    public DomainException(string code, string message, Exception innerException, int statusCode = 409)
        : base(message, innerException)
    {
        Code = code;
        StatusCode = statusCode;
    }

    public string Code { get; }

    public int StatusCode { get; }
}
