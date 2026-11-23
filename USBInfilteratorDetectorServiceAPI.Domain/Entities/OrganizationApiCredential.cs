namespace USBInfilteratorDetectorServiceAPI.Domain.Entities;

/// <summary>
/// Stores the hashed Worker API key for an organization. Plaintext keys are never persisted.
/// </summary>
public class OrganizationApiCredential
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public string ApiKeyHash { get; set; } = string.Empty;

    /// <summary>
    /// Non-secret prefix used for display and coarse lookup hints.
    /// </summary>
    public string ApiKeyPrefix { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
    public DateTime? RevokedAt { get; set; }
    public bool IsActive { get; set; }

    public Organization Organization { get; set; } = null!;
}
