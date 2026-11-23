namespace USBInfilteratorDetectorServiceAPI.Services.Abstractions;

/// <summary>
/// Sends email notifications (development stub when disabled).
/// </summary>
public interface IEmailNotificationSender
{
    /// <summary>
    /// Attempts to send an email notification.
    /// </summary>
    Task<(bool Success, string? Error)> SendAsync(string toEmail, string subject, string body, CancellationToken cancellationToken = default);
}
