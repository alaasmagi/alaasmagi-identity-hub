namespace Web.Contracts.Responses;

/// <summary>
/// Describes an API error response.
/// </summary>
public sealed class ErrorResponse
{
    /// <summary>
    /// Initializes a new error response.
    /// </summary>
    /// <param name="error">The error code or message.</param>
    public ErrorResponse(string error)
    {
        Error = error;
    }

    /// <summary>
    /// Gets the error code or message.
    /// </summary>
    public string Error { get; }
}
