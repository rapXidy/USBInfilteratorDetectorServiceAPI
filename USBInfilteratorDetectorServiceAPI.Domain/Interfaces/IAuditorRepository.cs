using USBInfilteratorDetectorServiceAPI.Domain.Entities;
using USBInfilteratorDetectorServiceAPI.Domain.Enums;

namespace USBInfilteratorDetectorServiceAPI.Domain.Interfaces;

/// <summary>
/// Persistence operations for organization auditor users.
/// </summary>
public interface IAuditorRepository
{
    /// <summary>
    /// Adds a new auditor user.
    /// </summary>
    Task AddAsync(Auditor auditor, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an auditor by identifier within an organization.
    /// </summary>
    Task<Auditor?> GetByIdAsync(Guid organizationId, Guid auditorId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an auditor by username within an organization.
    /// </summary>
    Task<Auditor?> GetByUsernameAsync(Guid organizationId, string username, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists auditors for an organization.
    /// </summary>
    Task<IReadOnlyList<Auditor>> ListByOrganizationAsync(Guid organizationId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Counts active admins for an organization.
    /// </summary>
    Task<int> CountActiveAdminsAsync(Guid organizationId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns whether a username already exists in the organization.
    /// </summary>
    Task<bool> UsernameExistsAsync(Guid organizationId, string username, Guid? excludeAuditorId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a tracked auditor entity.
    /// </summary>
    void Update(Auditor auditor);
}
