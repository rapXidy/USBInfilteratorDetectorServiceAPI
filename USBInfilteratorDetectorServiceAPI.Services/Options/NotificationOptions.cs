namespace USBInfilteratorDetectorServiceAPI.Services.Options;

/// <summary>
/// Notification pipeline configuration.
/// </summary>
public class NotificationOptions
{
    public const string SectionName = "Notifications";
    public EmailNotificationOptions Email { get; set; } = new();
    public NotificationDispatchOptions Dispatch { get; set; } = new();
    public int HighRiskThreshold { get; set; } = 50;
}

/// <summary>
/// Email notification toggle for development and production.
/// </summary>
public class EmailNotificationOptions
{
    public bool Enabled { get; set; }
    public string? FromAddress { get; set; }
}

/// <summary>
/// Background dispatch worker knobs (from appsettings).
/// </summary>
public class NotificationDispatchOptions
{
    /// <summary>
    /// Maximum send attempts before a notification is permanently Failed.
    /// </summary>
    public int MaxAttempts { get; set; } = 2;

    /// <summary>
    /// Maximum pending notifications processed per poll cycle.
    /// </summary>
    public int BatchSize { get; set; } = 50;

    /// <summary>
    /// Seconds between background dispatch poll cycles.
    /// </summary>
    public int PollingIntervalSeconds { get; set; } = 60;
}
