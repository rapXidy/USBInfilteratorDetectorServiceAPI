using USBInfilteratorDetectorServiceAPI.Domain.Entities;

namespace USBInfilteratorDetectorServiceAPI.Domain.Interfaces;

/// <summary>
/// Persistence operations for HR staff records and leaver USB-session correlation.
/// </summary>
public interface IHrStaffRepository
{
    /// <summary>
    /// Adds a new HR staff record.
    /// </summary>
    Task AddAsync(HrStaff staff, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a staff record by id within an organization.
    /// </summary>
    Task<HrStaff?> GetByIdAsync(Guid organizationId, Guid staffId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists staff for an organization with optional filters.
    /// </summary>
    Task<IReadOnlyList<HrStaff>> ListAsync(
        Guid organizationId,
        string? department = null,
        bool? resigningOnly = null,
        bool? isActive = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Counts staff with resignation set and exit still null.
    /// </summary>
    Task<int> CountResigningAsync(Guid organizationId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns whether an employee number already exists in the organization.
    /// </summary>
    Task<bool> EmployeeNumberExistsAsync(Guid organizationId, string employeeNumber, Guid? excludeStaffId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Queries USB sessions correlated to a single staff member.
    /// </summary>
    Task<IReadOnlyList<UsbSession>> QuerySessionsForStaffAsync(
        Guid organizationId,
        HrStaff staff,
        DateTime? fromUtc,
        DateTime? toUtc,
        int? minRiskScore,
        int skip,
        int take,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Queries USB sessions correlated to resigning and/or exited staff.
    /// </summary>
    Task<IReadOnlyList<(UsbSession Session, HrStaff Staff)>> QueryLeaverSessionsAsync(
        Guid organizationId,
        bool includeExited,
        DateTime? fromUtc,
        DateTime? toUtc,
        int? minRiskScore,
        int skip,
        int take,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Counts recent USB sessions for resigning staff (exit still null).
    /// </summary>
    Task<int> CountRecentResigningSessionsAsync(Guid organizationId, DateTime sinceUtc, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a tracked staff entity.
    /// </summary>
    void Update(HrStaff staff);
}
