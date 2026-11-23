using USBInfilteratorDetectorServiceAPI.Domain.Entities;

namespace USBInfilteratorDetectorServiceAPI.Domain.Interfaces;

/// <summary>
/// Persistence operations for organizations.
/// </summary>
public interface IOrganizationRepository
{
    /// <summary>
    /// Adds a new organization.
    /// </summary>
    Task AddAsync(Organization organization, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an organization by identifier.
    /// </summary>
    Task<Organization?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an organization by unique corporate code.
    /// </summary>
    Task<Organization?> GetByCorpCodeAsync(string corpCode, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists organizations optionally filtered by active flag.
    /// </summary>
    Task<IReadOnlyList<Organization>> ListAsync(bool? isActive = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns whether a corporate code is already assigned.
    /// </summary>
    Task<bool> CorpCodeExistsAsync(string corpCode, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a tracked organization entity.
    /// </summary>
    void Update(Organization organization);
}
