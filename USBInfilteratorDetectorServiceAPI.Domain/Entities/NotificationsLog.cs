using USBInfilteratorDetectorServiceAPI.Domain.Enums;

namespace USBInfilteratorDetectorServiceAPI.Domain.Entities;

/// <summary>
/// Audit log of notification delivery attempts for an organization.
/// </summary>
public class NotificationsLog
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public Guid? UsbSessionId { get; set; }
    public string Severity { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public NotificationChannel Channel { get; set; }
    public NotificationStatus Status { get; set; }
    public DateTime? SentAt { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime CreatedAt { get; set; }

    public Organization Organization { get; set; } = null!;
}
