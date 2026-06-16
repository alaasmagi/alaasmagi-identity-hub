using Application.Admin;
using Application.Auth;
using Application.Common.Auth;
using Application.Consent;
using Application.ExternalAuth;
using Application.TwoFactor;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<AuthWorkflow>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IConsentService, ConsentService>();
        services.AddScoped<ITwoFactorService, TwoFactorService>();
        services.AddScoped<IExternalAuthService, ExternalAuthService>();
        services.AddScoped<IAdminService, AdminService>();

        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        return services;
    }
}
