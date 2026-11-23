namespace USBInfilteratorDetectorServiceAPI.Domain.Entities;

/// <summary>
/// USB device serial approved for use within an organization.
/// </summary>
public class AllowedDevice
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public string DeviceSerial { get; set; } = string.Empty;
    public string? Vendor { get; set; }
    public string? Model { get; set; }
    public string? Description { get; set; }
    public Guid? AddedByAuditorId { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }

    public Organization Organization { get; set; } = null!;
}
