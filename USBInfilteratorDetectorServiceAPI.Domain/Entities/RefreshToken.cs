namespace USBInfilteratorDetectorServiceAPI.Domain.Entities;

/// <summary>
/// Refresh token issued to an auditor for access-token rotation.
/// </summary>
public class RefreshToken
{
    public Guid Id { get; set; }
    public Guid AuditorId { get; set; }
    public string TokenHash { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? RevokedAt { get; set; }

    public Auditor Auditor { get; set; } = null!;
}
