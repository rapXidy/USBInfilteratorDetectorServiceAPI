using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using USBInfilteratorDetectorServiceAPI.Services.Abstractions;
using USBInfilteratorDetectorServiceAPI.Services.Options;

namespace USBInfilteratorDetectorServiceAPI.Infrastructure.Notifications;

/// <summary>
/// Development-friendly email sender that records failure when email is disabled.
/// </summary>
public class EmailNotificationSender : IEmailNotificationSender
{
    private readonly NotificationOptions _options;
    private readonly ILogger<EmailNotificationSender> _logger;

    public EmailNotificationSender(IOptions<NotificationOptions> options, ILogger<EmailNotificationSender> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    /// <inheritdoc />
    public Task<(bool Success, string? Error)> SendAsync(string toEmail, string subject, string body, CancellationToken cancellationToken = default)
    {
        if (!_options.Email.Enabled)
        {
            _logger.LogInformation("Email disabled. Would send to {To}: {Subject}", toEmail, subject);
            return Task.FromResult<(bool, string?)>((false, "Email notifications are disabled."));
        }

        // Provider wiring (Resend/Brevo/Mailtrap) is intentionally stubbed for v1.
        _logger.LogInformation("Email stub accepted message to {To}: {Subject}", toEmail, subject);
        return Task.FromResult<(bool, string?)>((true, null));
    }
}
