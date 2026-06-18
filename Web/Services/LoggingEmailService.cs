using Application.Common.Abstractions;
using Domain;

namespace Web.Services;

public sealed class LoggingEmailService : IEmailService
{
    private readonly ILogger<LoggingEmailService> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public LoggingEmailService(ILogger<LoggingEmailService> logger, IHttpContextAccessor httpContextAccessor)
    {
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }

    public Task SendEmailConfirmationAsync(AppUser user, string token)
    {
        var confirmationUrl = BuildUrl($"/Identity/Account/ConfirmEmail?userId={user.Id}&token={Uri.EscapeDataString(token)}");
        _logger.LogInformation(
            "Email confirmation URL generated for user {UserId} ({Email}): {ConfirmationUrl}",
            user.Id,
            user.Email,
            confirmationUrl);

        return Task.CompletedTask;
    }

    public Task SendPasswordResetAsync(AppUser user, string token)
    {
        _logger.LogInformation(
            "Password reset token generated for user {UserId} ({Email}): {Token}",
            user.Id,
            user.Email,
            token);

        return Task.CompletedTask;
    }

    private string BuildUrl(string pathAndQuery)
    {
        var request = _httpContextAccessor.HttpContext?.Request;
        return request is null
            ? pathAndQuery
            : $"{request.Scheme}://{request.Host}{pathAndQuery}";
    }
}
