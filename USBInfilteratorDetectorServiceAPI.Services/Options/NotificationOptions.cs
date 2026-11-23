namespace USBInfilteratorDetectorServiceAPI.Services.Options;

/// <summary>
/// Notification pipeline configuration.
/// </summary>
public class NotificationOptions
{
    public const string SectionName = "Notifications";
    public EmailNotificationOptions Email { get; set; } = new();
    public int HighRiskThreshold { get; set; } = 50;
}

/// <summary>
/// Email notification toggle for development.
/// </summary>
public class EmailNotificationOptions
{
    public bool Enabled { get; set; }
    public string? FromAddress { get; set; }
}
