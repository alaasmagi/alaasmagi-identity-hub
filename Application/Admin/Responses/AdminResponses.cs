namespace Application.Admin.Responses;

public sealed record UserSummary(
    Guid UserId,
    string Email,
    string FullName,
    bool IsActive,
    bool IsBanned,
    string? BanReason);
