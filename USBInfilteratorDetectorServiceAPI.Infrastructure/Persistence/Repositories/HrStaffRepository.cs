using Microsoft.EntityFrameworkCore;
using USBInfilteratorDetectorServiceAPI.Domain.Entities;
using USBInfilteratorDetectorServiceAPI.Domain.Interfaces;

namespace USBInfilteratorDetectorServiceAPI.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF Core repository for HR staff and leaver USB-session correlation.
/// </summary>
public class HrStaffRepository : IHrStaffRepository
{
    private readonly AppDbContext _db;

    public HrStaffRepository(AppDbContext db) => _db = db;

    /// <inheritdoc />
    public async Task AddAsync(HrStaff staff, CancellationToken cancellationToken = default)
        => await _db.HrStaff.AddAsync(staff, cancellationToken);

    /// <inheritdoc />
    public Task<HrStaff?> GetByIdAsync(Guid organizationId, Guid staffId, CancellationToken cancellationToken = default)
        => _db.HrStaff.FirstOrDefaultAsync(x => x.OrganizationId == organizationId && x.Id == staffId, cancellationToken);

    /// <inheritdoc />
    public async Task<IReadOnlyList<HrStaff>> ListAsync(
        Guid organizationId,
        string? department = null,
        bool? resigningOnly = null,
        bool? isActive = null,
        CancellationToken cancellationToken = default)
    {
        var query = _db.HrStaff.Where(x => x.OrganizationId == organizationId);

        if (!string.IsNullOrWhiteSpace(department))
            query = query.Where(x => x.Department.Contains(department));

        if (resigningOnly == true)
            query = query.Where(x => x.DateOfResignation != null && x.DateOfExit == null);

        if (isActive.HasValue)
            query = query.Where(x => x.IsActive == isActive.Value);

        return await query
            .OrderBy(x => x.FullName)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public Task<int> CountResigningAsync(Guid organizationId, CancellationToken cancellationToken = default)
        => _db.HrStaff.CountAsync(
            x => x.OrganizationId == organizationId
                 && x.IsActive
                 && x.DateOfResignation != null
                 && x.DateOfExit == null,
            cancellationToken);

    /// <inheritdoc />
    public Task<bool> EmployeeNumberExistsAsync(Guid organizationId, string employeeNumber, Guid? excludeStaffId = null, CancellationToken cancellationToken = default)
    {
        var query = _db.HrStaff.Where(x => x.OrganizationId == organizationId && x.EmployeeNumber == employeeNumber);
        if (excludeStaffId.HasValue)
            query = query.Where(x => x.Id != excludeStaffId.Value);
        return query.AnyAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<UsbSession>> QuerySessionsForStaffAsync(
        Guid organizationId,
        HrStaff staff,
        DateTime? fromUtc,
        DateTime? toUtc,
        int? minRiskScore,
        int skip,
        int take,
        CancellationToken cancellationToken = default)
    {
        var query = BuildMatchedSessionsQuery(organizationId, new[] { staff });
        query = ApplySessionFilters(query, fromUtc, toUtc, minRiskScore);

        return await query
            .Include(x => x.Host)
            .OrderByDescending(x => x.ConnectedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<(UsbSession Session, HrStaff Staff)>> QueryLeaverSessionsAsync(
        Guid organizationId,
        bool includeExited,
        DateTime? fromUtc,
        DateTime? toUtc,
        int? minRiskScore,
        int skip,
        int take,
        CancellationToken cancellationToken = default)
    {
        var leaversQuery = _db.HrStaff.Where(x =>
            x.OrganizationId == organizationId
            && x.IsActive
            && x.DateOfResignation != null);

        if (!includeExited)
            leaversQuery = leaversQuery.Where(x => x.DateOfExit == null);

        var leavers = await leaversQuery.ToListAsync(cancellationToken);
        if (leavers.Count == 0)
            return Array.Empty<(UsbSession, HrStaff)>();

        var sessions = await ApplySessionFilters(BuildMatchedSessionsQuery(organizationId, leavers), fromUtc, toUtc, minRiskScore)
            .Include(x => x.Host)
            .OrderByDescending(x => x.ConnectedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);

        return sessions
            .Select(session => (session, MatchStaff(leavers, session)!))
            .Where(x => x.Item2 is not null)
            .Select(x => (x.session, x.Item2))
            .ToList();
    }

    /// <inheritdoc />
    public async Task<int> CountRecentResigningSessionsAsync(Guid organizationId, DateTime sinceUtc, CancellationToken cancellationToken = default)
    {
        var leavers = await _db.HrStaff
            .Where(x =>
                x.OrganizationId == organizationId
                && x.IsActive
                && x.DateOfResignation != null
                && x.DateOfExit == null)
            .ToListAsync(cancellationToken);

        if (leavers.Count == 0)
            return 0;

        return await BuildMatchedSessionsQuery(organizationId, leavers)
            .Where(x => x.ConnectedAt >= sinceUtc)
            .CountAsync(cancellationToken);
    }

    /// <inheritdoc />
    public void Update(HrStaff staff) => _db.HrStaff.Update(staff);

    private IQueryable<UsbSession> BuildMatchedSessionsQuery(Guid organizationId, IReadOnlyList<HrStaff> staffMembers)
    {
        var usernames = staffMembers
            .Select(x => x.WindowsUsername)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x!.Trim().ToLowerInvariant())
            .Distinct()
            .ToList();

        var deviceIds = staffMembers
            .Select(x => x.DeviceId)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x!.Trim())
            .Distinct()
            .ToList();

        return _db.UsbSessions
            .Where(s => s.OrganizationId == organizationId)
            .Where(s =>
                (s.Host.LoggedInUser != null && usernames.Contains(s.Host.LoggedInUser.ToLower()))
                || deviceIds.Contains(s.Host.DeviceId));
    }

    private static IQueryable<UsbSession> ApplySessionFilters(
        IQueryable<UsbSession> query,
        DateTime? fromUtc,
        DateTime? toUtc,
        int? minRiskScore)
    {
        if (fromUtc.HasValue)
            query = query.Where(x => x.ConnectedAt >= fromUtc.Value);
        if (toUtc.HasValue)
            query = query.Where(x => x.ConnectedAt <= toUtc.Value);
        if (minRiskScore.HasValue)
            query = query.Where(x => x.RiskScore >= minRiskScore.Value);
        return query;
    }

    private static HrStaff? MatchStaff(IReadOnlyList<HrStaff> leavers, UsbSession session)
    {
        var loggedIn = session.Host.LoggedInUser?.Trim();
        if (!string.IsNullOrWhiteSpace(loggedIn))
        {
            var byUser = leavers.FirstOrDefault(s =>
                !string.IsNullOrWhiteSpace(s.WindowsUsername)
                && string.Equals(s.WindowsUsername.Trim(), loggedIn, StringComparison.OrdinalIgnoreCase));
            if (byUser is not null)
                return byUser;
        }

        return leavers.FirstOrDefault(s =>
            !string.IsNullOrWhiteSpace(s.DeviceId)
            && string.Equals(s.DeviceId.Trim(), session.Host.DeviceId, StringComparison.OrdinalIgnoreCase));
    }
}
