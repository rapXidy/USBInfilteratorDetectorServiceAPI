namespace USBInfilteratorDetectorServiceAPI.Domain.Entities;

/// <summary>
/// Reconstructed USB insertion session with risk score and detection status.
/// </summary>
public class UsbSession
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public Guid HostId { get; set; }
    public Guid SessionId { get; set; }
    public string DeviceSerial { get; set; } = string.Empty;
    public string? Vendor { get; set; }
    public string? Model { get; set; }
    public DateTime ConnectedAt { get; set; }
    public DateTime? DisconnectedAt { get; set; }
    public long TotalBytesWritten { get; set; }
    public int RiskScore { get; set; }
    public bool DetectionComplete { get; set; }
    public DateTime CreatedAt { get; set; }

    public Organization Organization { get; set; } = null!;
    public Host Host { get; set; } = null!;
    public ICollection<SessionReason> Reasons { get; set; } = new List<SessionReason>();
    public ICollection<RawEvent> Events { get; set; } = new List<RawEvent>();
}
