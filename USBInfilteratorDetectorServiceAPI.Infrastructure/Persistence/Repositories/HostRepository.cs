using Microsoft.EntityFrameworkCore;
using USBInfilteratorDetectorServiceAPI.Domain.Entities;
using USBInfilteratorDetectorServiceAPI.Domain.Interfaces;

namespace USBInfilteratorDetectorServiceAPI.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF Core repository for hosts.
/// </summary>
public class HostRepository : IHostRepository
{
    private readonly AppDbContext _db;

    public HostRepository(AppDbContext db) => _db = db;

    /// <inheritdoc />
    public async Task AddAsync(Host host, CancellationToken cancellationToken = default)
        => await _db.Hosts.AddAsync(host, cancellationToken);

    /// <inheritdoc />
    public Task<Host?> GetByDeviceIdAsync(Guid organizationId, string deviceId, CancellationToken cancellationToken = default)
        => _db.Hosts.FirstOrDefaultAsync(x => x.OrganizationId == organizationId && x.DeviceId == deviceId, cancellationToken);

    /// <inheritdoc />
    public Task<int> CountByOrganizationAsync(Guid organizationId, CancellationToken cancellationToken = default)
        => _db.Hosts.CountAsync(x => x.OrganizationId == organizationId, cancellationToken);

    /// <inheritdoc />
    public void Update(Host host) => _db.Hosts.Update(host);
}
