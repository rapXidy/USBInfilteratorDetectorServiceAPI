namespace USBInfilteratorDetectorServiceAPI.Domain.Enums;

/// <summary>
/// Delivery outcome for a notification attempt.
/// </summary>
public enum NotificationStatus
{
    Pending = 0,
    Sent = 1,
    Failed = 2
}
