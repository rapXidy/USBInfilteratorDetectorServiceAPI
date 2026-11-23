namespace USBInfilteratorDetectorServiceAPI.Domain.Entities;

/// <summary>
/// Human-resources staff record used to track leavers and correlate USB sessions.
/// </summary>
public class HrStaff
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public string? JobTitle { get; set; }
    public string? EmployeeNumber { get; set; }

    /// <summary>
    /// Windows account used to match <see cref="Host.LoggedInUser"/>.
    /// </summary>
    public string? WindowsUsername { get; set; }

    /// <summary>
    /// Assigned host machine id used to match <see cref="Host.DeviceId"/>.
    /// </summary>
    public string? DeviceId { get; set; }

    public DateTime DateEmployed { get; set; }
    public DateTime? DateOfResignation { get; set; }
    public DateTime? DateOfExit { get; set; }
    public bool IsActive { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? CreatedByAuditorId { get; set; }

    public Organization Organization { get; set; } = null!;
}
