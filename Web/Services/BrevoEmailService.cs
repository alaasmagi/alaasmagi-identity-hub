using System.Net;
using System.Net.Http.Json;
using System.Text;
using Application.Common.Abstractions;
using Domain;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;

namespace Web.Services;

public sealed class BrevoEmailService : IEmailService
{
    private static readonly Uri SendEmailUri = new("https://api.brevo.com/v3/smtp/email");

    private readonly HttpClient _httpClient;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<BrevoEmailService> _logger;
    private readonly BrevoEmailOptions _options;

    public BrevoEmailService(
        HttpClient httpClient,
        IHttpContextAccessor httpContextAccessor,
        IWebHostEnvironment environment,
        ILogger<BrevoEmailService> logger,
        IOptions<BrevoEmailOptions> options)
    {
        _httpClient = httpClient;
        _httpContextAccessor = httpContextAccessor;
        _environment = environment;
        _logger = logger;
        _options = options.Value;
    }

    public Task SendEmailConfirmationAsync(AppUser user, string token)
    {
        var code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
        var confirmationUrl = BuildUrl($"/Identity/Account/ConfirmEmail?userId={user.Id}&code={code}");
        var html = RenderTemplate("ConfirmEmail.html", new Dictionary<string, string>
        {
            ["ApplicationName"] = "Identity Hub",
            ["FullName"] = DisplayName(user),
            ["ConfirmationUrl"] = confirmationUrl
        });

        return SendAsync(user, "Confirm your email address", html);
    }

    public Task SendPasswordResetAsync(AppUser user, string token)
    {
        var code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
        var resetUrl = BuildUrl($"/Identity/Account/ResetPassword?userId={user.Id}&code={code}");
        var html = $"""
            <p>Hello {WebUtility.HtmlEncode(DisplayName(user))},</p>
            <p>Reset your password by opening this link:</p>
            <p><a href="{WebUtility.HtmlEncode(resetUrl)}">Reset password</a></p>
            <p>If you did not request a password reset, you can ignore this email.</p>
            """;

        return SendAsync(user, "Reset your password", html);
    }

    private async Task SendAsync(AppUser user, string subject, string htmlContent)
    {
        if (string.IsNullOrWhiteSpace(user.Email))
        {
            _logger.LogWarning("Skipping Brevo email for user {UserId} because no email address is available.", user.Id);
            return;
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, SendEmailUri)
        {
            Content = JsonContent.Create(new
            {
                sender = new
                {
                    email = _options.SenderEmail
                },
                to = new[]
                {
                    new
                    {
                        email = user.Email,
                        name = DisplayName(user)
                    }
                },
                subject,
                htmlContent
            })
        };

        request.Headers.Add("api-key", _options.ApiKey);
        var response = await _httpClient.SendAsync(request);
        var responseBody = await response.Content.ReadAsStringAsync();
        if (response.IsSuccessStatusCode)
        {
            _logger.LogInformation(
                "Brevo email '{Subject}' accepted for user {UserId} ({Email}): {ResponseBody}",
                subject,
                user.Id,
                user.Email,
                responseBody);
            return;
        }

        _logger.LogError(
            "Brevo email '{Subject}' failed for user {UserId} ({Email}) with status {StatusCode}: {ResponseBody}",
            subject,
            user.Id,
            user.Email,
            response.StatusCode,
            responseBody);

        response.EnsureSuccessStatusCode();
    }

    private string BuildUrl(string pathAndQuery)
    {
        var request = _httpContextAccessor.HttpContext?.Request;
        return request is null
            ? pathAndQuery
            : $"{request.Scheme}://{request.Host}{pathAndQuery}";
    }

    private string RenderTemplate(string fileName, IReadOnlyDictionary<string, string> parameters)
    {
        var templatePath = Path.Combine(_environment.ContentRootPath, "EmailTemplates", fileName);
        var html = File.ReadAllText(templatePath);

        foreach (var parameter in parameters)
        {
            html = html.Replace(
                "{{" + parameter.Key + "}}",
                WebUtility.HtmlEncode(parameter.Value),
                StringComparison.Ordinal);
        }

        return html;
    }

    private static string DisplayName(AppUser user)
    {
        return string.IsNullOrWhiteSpace(user.FullName) ? user.Email ?? user.Id.ToString() : user.FullName;
    }
}
