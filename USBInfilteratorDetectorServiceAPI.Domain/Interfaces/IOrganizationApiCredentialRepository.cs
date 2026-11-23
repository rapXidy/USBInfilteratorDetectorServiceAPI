using USBInfilteratorDetectorServiceAPI.Domain.Entities;

namespace USBInfilteratorDetectorServiceAPI.Domain.Interfaces;

/// <summary>
/// Persistence operations for organization Worker API credentials.
/// </summary>
public interface IOrganizationApiCredentialRepository
{
    /// <summary>
    /// Adds a new API credential row.
    /// </summary>
    Task AddAsync(OrganizationApiCredential credential, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the active credential for an organization.
    /// </summary>
    Task<OrganizationApiCredential?> GetActiveByOrganizationIdAsync(Guid organizationId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds an active credential by API key prefix for verification.
    /// </summary>
    Task<IReadOnlyList<OrganizationApiCredential>> GetActiveByPrefixAsync(string apiKeyPrefix, CancellationToken cancellationToken = default);
}
