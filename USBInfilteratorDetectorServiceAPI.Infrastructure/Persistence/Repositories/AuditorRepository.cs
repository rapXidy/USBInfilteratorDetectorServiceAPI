using Microsoft.EntityFrameworkCore;
using USBInfilteratorDetectorServiceAPI.Domain.Entities;
using USBInfilteratorDetectorServiceAPI.Domain.Enums;
using USBInfilteratorDetectorServiceAPI.Domain.Interfaces;

namespace USBInfilteratorDetectorServiceAPI.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF Core repository for auditor users.
/// </summary>
public class AuditorRepository : IAuditorRepository
{
    private readonly AppDbContext _db;

    public AuditorRepository(AppDbContext db) => _db = db;

    /// <inheritdoc />
    public async Task AddAsync(Auditor auditor, CancellationToken cancellationToken = default)
        => await _db.Auditors.AddAsync(auditor, cancellationToken);

    /// <inheritdoc />
    public Task<Auditor?> GetByIdAsync(Guid organizationId, Guid auditorId, CancellationToken cancellationToken = default)
        => _db.Auditors.FirstOrDefaultAsync(x => x.OrganizationId == organizationId && x.Id == auditorId, cancellationToken);

    /// <inheritdoc />
    public Task<Auditor?> GetByUsernameAsync(Guid organizationId, string username, CancellationToken cancellationToken = default)
        => _db.Auditors
            .Include(x => x.Organization)
            .FirstOrDefaultAsync(x => x.OrganizationId == organizationId && x.Username == username, cancellationToken);

    /// <inheritdoc />
    public async Task<IReadOnlyList<Auditor>> ListByOrganizationAsync(Guid organizationId, CancellationToken cancellationToken = default)
        => await _db.Auditors
            .Where(x => x.OrganizationId == organizationId)
            .OrderBy(x => x.Username)
            .ToListAsync(cancellationToken);

    /// <inheritdoc />
    public Task<int> CountActiveAdminsAsync(Guid organizationId, CancellationToken cancellationToken = default)
        => _db.Auditors.CountAsync(
            x => x.OrganizationId == organizationId && x.IsActive && x.Role == AuditorRole.Admin,
            cancellationToken);

    /// <inheritdoc />
    public Task<bool> UsernameExistsAsync(Guid organizationId, string username, Guid? excludeAuditorId = null, CancellationToken cancellationToken = default)
    {
        var query = _db.Auditors.Where(x => x.OrganizationId == organizationId && x.Username == username);
        if (excludeAuditorId.HasValue)
            query = query.Where(x => x.Id != excludeAuditorId.Value);
        return query.AnyAsync(cancellationToken);
    }

    /// <inheritdoc />
    public void Update(Auditor auditor) => _db.Auditors.Update(auditor);
}
