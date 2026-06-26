using System.Text;
using Application.Common.Abstractions;
using Domain;
using Microsoft.AspNetCore.WebUtilities;

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
        var code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
        var confirmationUrl = BuildUrl($"/Identity/Account/ConfirmEmail?userId={user.Id}&code={code}");
        _logger.LogInformation(
            "Email confirmation URL generated for user {UserId} ({Email}): {ConfirmationUrl}",
            user.Id,
            user.Email,
            confirmationUrl);

        return Task.CompletedTask;
    }

    public Task SendPasswordResetAsync(AppUser user, string token)
    {
        var code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
        var resetUrl = BuildUrl($"/Identity/Account/ResetPassword?userId={user.Id}&code={code}");
        _logger.LogInformation(
            "Password reset URL generated for user {UserId} ({Email}): {ResetUrl}",
            user.Id,
            user.Email,
            resetUrl);

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
