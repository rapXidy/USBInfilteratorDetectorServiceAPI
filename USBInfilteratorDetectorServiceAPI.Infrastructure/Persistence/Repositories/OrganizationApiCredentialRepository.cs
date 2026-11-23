using Microsoft.EntityFrameworkCore;
using USBInfilteratorDetectorServiceAPI.Domain.Entities;
using USBInfilteratorDetectorServiceAPI.Domain.Interfaces;

namespace USBInfilteratorDetectorServiceAPI.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF Core repository for organization API credentials.
/// </summary>
public class OrganizationApiCredentialRepository : IOrganizationApiCredentialRepository
{
    private readonly AppDbContext _db;

    public OrganizationApiCredentialRepository(AppDbContext db) => _db = db;

    /// <inheritdoc />
    public async Task AddAsync(OrganizationApiCredential credential, CancellationToken cancellationToken = default)
        => await _db.OrganizationApiCredentials.AddAsync(credential, cancellationToken);

    /// <inheritdoc />
    public Task<OrganizationApiCredential?> GetActiveByOrganizationIdAsync(Guid organizationId, CancellationToken cancellationToken = default)
        => _db.OrganizationApiCredentials
            .Include(x => x.Organization)
            .FirstOrDefaultAsync(x => x.OrganizationId == organizationId && x.IsActive && x.RevokedAt == null, cancellationToken);

    /// <inheritdoc />
    public async Task<IReadOnlyList<OrganizationApiCredential>> GetActiveByPrefixAsync(string apiKeyPrefix, CancellationToken cancellationToken = default)
        => await _db.OrganizationApiCredentials
            .Include(x => x.Organization)
            .Where(x => x.ApiKeyPrefix == apiKeyPrefix && x.IsActive && x.RevokedAt == null)
            .ToListAsync(cancellationToken);
}
