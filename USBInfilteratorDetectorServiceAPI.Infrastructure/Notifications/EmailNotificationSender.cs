using MailKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using USBInfilteratorDetectorServiceAPI.Services.Abstractions;
using USBInfilteratorDetectorServiceAPI.Services.Options;

namespace USBInfilteratorDetectorServiceAPI.Infrastructure.Notifications;

/// <summary>
/// Sends email via Gmail SMTP using MailKit when notifications email is enabled.
/// </summary>
public sealed class EmailNotificationSender : IEmailNotificationSender
{
    private readonly NotificationOptions _notificationOptions;
    private readonly EmailSettings _emailSettings;
    private readonly ILogger<EmailNotificationSender> _logger;

    public EmailNotificationSender(
        IOptions<NotificationOptions> notificationOptions,
        IOptions<EmailSettings> emailSettings,
        ILogger<EmailNotificationSender> logger)
    {
        _notificationOptions = notificationOptions.Value;
        _emailSettings = emailSettings.Value;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<(bool Success, string? Error)> SendAsync(
        string toEmail,
        string subject,
        string body,
        bool isHtml = true,
        CancellationToken cancellationToken = default)
    {
        if (!_notificationOptions.Email.Enabled)
        {
            _logger.LogInformation("Email disabled. Would send to {To}: {Subject}", toEmail, subject);
            return (false, "Email notifications are disabled.");
        }

        if (string.IsNullOrWhiteSpace(toEmail))
            return (false, "Destination email is empty.");

        if (string.IsNullOrWhiteSpace(_emailSettings.SmtpServer) ||
            string.IsNullOrWhiteSpace(_emailSettings.Username) ||
            string.IsNullOrWhiteSpace(_emailSettings.Password))
        {
            return (false, "Email SMTP configuration is incomplete.");
        }

        var senderEmail = !string.IsNullOrWhiteSpace(_emailSettings.SenderEmail)
            ? _emailSettings.SenderEmail
            : !string.IsNullOrWhiteSpace(_notificationOptions.Email.FromAddress)
                ? _notificationOptions.Email.FromAddress!
                : _emailSettings.Username;

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_emailSettings.SenderName, senderEmail));
        message.To.Add(MailboxAddress.Parse(toEmail.Trim()));
        message.Subject = subject;
        message.Body = isHtml
            ? new BodyBuilder { HtmlBody = body }.ToMessageBody()
            : new TextPart("plain") { Text = body };

        using var smtpClient = new SmtpClient();
        try
        {
            await smtpClient.ConnectAsync(
                _emailSettings.SmtpServer,
                _emailSettings.Port,
                SecureSocketOptions.StartTls,
                cancellationToken).ConfigureAwait(false);

            await smtpClient.AuthenticateAsync(
                _emailSettings.Username,
                _emailSettings.Password,
                cancellationToken).ConfigureAwait(false);

            await smtpClient.SendAsync(message, cancellationToken).ConfigureAwait(false);

            _logger.LogInformation("Email successfully sent to {Recipient} subject {Subject}", toEmail, subject);
            return (true, null);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Email delivery cancelled for {Recipient}", toEmail);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Recipient}", toEmail);
            return (false, "Email delivery failed.");
        }
        finally
        {
            if (smtpClient.IsConnected)
            {
                try
                {
                    await smtpClient.DisconnectAsync(true, CancellationToken.None).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to disconnect SMTP client cleanly.");
                }
            }
        }
    }
}
