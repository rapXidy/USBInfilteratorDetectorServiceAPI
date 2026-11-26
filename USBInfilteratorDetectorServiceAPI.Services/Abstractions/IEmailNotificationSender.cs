namespace USBInfilteratorDetectorServiceAPI.Services.Abstractions;

/// <summary>
/// Sends email notifications via the configured SMTP provider.
/// </summary>
public interface IEmailNotificationSender
{
    /// <summary>
    /// Attempts to send an email notification.
    /// </summary>
    Task<(bool Success, string? Error)> SendAsync(
        string toEmail,
        string subject,
        string body,
        bool isHtml = true,
        CancellationToken cancellationToken = default);
}
