using Microsoft.EntityFrameworkCore;
using USBInfilteratorDetectorServiceAPI.Domain.Entities;
using USBInfilteratorDetectorServiceAPI.Domain.Interfaces;

namespace USBInfilteratorDetectorServiceAPI.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF Core repository for allowed devices.
/// </summary>
public class AllowedDeviceRepository : IAllowedDeviceRepository
{
    private readonly AppDbContext _db;

    public AllowedDeviceRepository(AppDbContext db) => _db = db;

    /// <inheritdoc />
    public async Task AddAsync(AllowedDevice device, CancellationToken cancellationToken = default)
        => await _db.AllowedDevices.AddAsync(device, cancellationToken);

    /// <inheritdoc />
    public Task<AllowedDevice?> GetByIdAsync(Guid organizationId, Guid deviceId, CancellationToken cancellationToken = default)
        => _db.AllowedDevices.FirstOrDefaultAsync(x => x.OrganizationId == organizationId && x.Id == deviceId, cancellationToken);

    /// <inheritdoc />
    public async Task<IReadOnlyList<AllowedDevice>> ListByOrganizationAsync(Guid organizationId, bool includeInactive = false, CancellationToken cancellationToken = default)
    {
        var query = _db.AllowedDevices.Where(x => x.OrganizationId == organizationId);
        if (!includeInactive)
            query = query.Where(x => x.IsActive);
        return await query.OrderByDescending(x => x.CreatedAt).ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public void Update(AllowedDevice device) => _db.AllowedDevices.Update(device);
}
