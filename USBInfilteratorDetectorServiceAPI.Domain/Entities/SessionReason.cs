namespace USBInfilteratorDetectorServiceAPI.Domain.Entities;

/// <summary>
/// Explainable risk reason attached to a USB session.
/// </summary>
public class SessionReason
{
    public Guid Id { get; set; }
    public Guid UsbSessionId { get; set; }
    public string RuleName { get; set; } = string.Empty;
    public int Weight { get; set; }
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// JSON array of Windows Event Record IDs that triggered this reason.
    /// </summary>
    public string EventRecordIdsJson { get; set; } = "[]";

    public UsbSession UsbSession { get; set; } = null!;
}
