using USBInfilteratorDetectorServiceAPI.Domain.Enums;

namespace USBInfilteratorDetectorServiceAPI.Domain.Entities;

/// <summary>
/// Multi-tenant organization (customer) that owns hosts, auditors, and USB sessions.
/// </summary>
public class Organization
{
    public Guid Id { get; set; }

    /// <summary>
    /// Unique corporate code issued when onboarding completes. Null while Draft.
    /// </summary>
    public string? CorpCode { get; set; }

    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? OfficeAddress { get; set; }
    public string? Sector { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }
    public OnboardingStatus OnboardingStatus { get; set; }
    public DateTime? OnboardingCompletedAt { get; set; }

    public ICollection<Auditor> Auditors { get; set; } = new List<Auditor>();
    public ICollection<OrganizationApiCredential> ApiCredentials { get; set; } = new List<OrganizationApiCredential>();
    public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
    public ICollection<AllowedDevice> AllowedDevices { get; set; } = new List<AllowedDevice>();
    public ICollection<HrStaff> HrStaffMembers { get; set; } = new List<HrStaff>();
    public ICollection<Host> Hosts { get; set; } = new List<Host>();
    public ICollection<UsbSession> UsbSessions { get; set; } = new List<UsbSession>();
    public ICollection<NotificationsLog> Notifications { get; set; } = new List<NotificationsLog>();
}
