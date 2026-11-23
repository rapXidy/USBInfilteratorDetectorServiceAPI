using Microsoft.EntityFrameworkCore;
using USBInfilteratorDetectorServiceAPI.Domain.Entities;
using USBInfilteratorDetectorServiceAPI.Domain.Interfaces;

namespace USBInfilteratorDetectorServiceAPI.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF Core repository for organizations.
/// </summary>
public class OrganizationRepository : IOrganizationRepository
{
    private readonly AppDbContext _db;

    public OrganizationRepository(AppDbContext db) => _db = db;

    /// <inheritdoc />
    public async Task AddAsync(Organization organization, CancellationToken cancellationToken = default)
        => await _db.Organizations.AddAsync(organization, cancellationToken);

    /// <inheritdoc />
    public Task<Organization?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => _db.Organizations
            .Include(x => x.ApiCredentials)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    /// <inheritdoc />
    public Task<Organization?> GetByCorpCodeAsync(string corpCode, CancellationToken cancellationToken = default)
        => _db.Organizations
            .Include(x => x.ApiCredentials)
            .FirstOrDefaultAsync(x => x.CorpCode == corpCode, cancellationToken);

    /// <inheritdoc />
    public async Task<IReadOnlyList<Organization>> ListAsync(bool? isActive = null, CancellationToken cancellationToken = default)
    {
        var query = _db.Organizations.Include(x => x.ApiCredentials).AsQueryable();
        if (isActive.HasValue)
            query = query.Where(x => x.IsActive == isActive.Value);

        return await query.OrderByDescending(x => x.CreatedAt).ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public Task<bool> CorpCodeExistsAsync(string corpCode, CancellationToken cancellationToken = default)
        => _db.Organizations.AnyAsync(x => x.CorpCode == corpCode, cancellationToken);

    /// <inheritdoc />
    public void Update(Organization organization) => _db.Organizations.Update(organization);
}
