namespace USBInfilteratorDetectorServiceAPI.Domain.Entities;

/// <summary>
/// Monitored Windows endpoint that reports USB sessions to the API.
/// </summary>
public class Host
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public string HostName { get; set; } = string.Empty;
    public string DeviceId { get; set; } = string.Empty;
    public DateTime LastSeenAt { get; set; }
    public string? OsVersion { get; set; }
    public string? LoggedInUser { get; set; }

    public Organization Organization { get; set; } = null!;
    public ICollection<UsbSession> UsbSessions { get; set; } = new List<UsbSession>();
}
