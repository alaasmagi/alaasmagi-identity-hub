namespace Application.Admin.Responses;

/// <summary>
/// Response item summarizing a user for admin screens.
/// </summary>
/// <param name="UserId">The user identifier.</param>
/// <param name="Email">The user email address.</param>
/// <param name="FullName">The user full name.</param>
/// <param name="IsActive">Whether the user is active.</param>
/// <param name="IsBanned">Whether the user is banned.</param>
/// <param name="BanReason">The optional ban reason.</param>
public sealed record UserSummary(
    Guid UserId,
    string Email,
    string FullName,
    bool IsActive,
    bool IsBanned,
    string? BanReason);
