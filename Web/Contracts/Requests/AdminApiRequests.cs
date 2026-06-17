namespace Web.Contracts.Requests;

/// <summary>
/// Request body for banning a user.
/// </summary>
public sealed class BanUserApiRequest
{
    /// <summary>
    /// Gets or sets the ban reason.
    /// </summary>
    public string Reason { get; set; } = default!;
}
