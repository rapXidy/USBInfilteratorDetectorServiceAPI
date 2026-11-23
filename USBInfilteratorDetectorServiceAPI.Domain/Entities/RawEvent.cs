namespace USBInfilteratorDetectorServiceAPI.Domain.Entities;

/// <summary>
/// Raw forensic event captured during a USB session.
/// </summary>
public class RawEvent
{
    public Guid Id { get; set; }
    public Guid UsbSessionId { get; set; }
    public int EventId { get; set; }
    public long RecordId { get; set; }
    public DateTime TimeCreated { get; set; }
    public string Activity { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? RawJson { get; set; }

    public UsbSession UsbSession { get; set; } = null!;
}
