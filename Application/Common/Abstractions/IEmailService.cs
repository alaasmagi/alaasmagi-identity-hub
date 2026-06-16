using Domain;

namespace Application.Common.Abstractions;

public interface IEmailService
{
    Task SendEmailConfirmationAsync(AppUser user, string token);
    Task SendPasswordResetAsync(AppUser user, string token);
}
