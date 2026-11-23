namespace USBInfilteratorDetectorServiceAPI.Domain.Enums;

/// <summary>
/// Delivery channel used for a notification attempt.
/// </summary>
public enum NotificationChannel
{
    Email = 0,
    Webhook = 1,
    Api = 2
}
