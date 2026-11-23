using USBInfilteratorDetectorServiceAPI.Domain.Entities;

namespace USBInfilteratorDetectorServiceAPI.Domain.Interfaces;

/// <summary>
/// Persistence operations for allowed USB devices.
/// </summary>
public interface IAllowedDeviceRepository
{
    Task AddAsync(AllowedDevice device, CancellationToken cancellationToken = default);
    Task<AllowedDevice?> GetByIdAsync(Guid organizationId, Guid deviceId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AllowedDevice>> ListByOrganizationAsync(Guid organizationId, bool includeInactive = false, CancellationToken cancellationToken = default);
    void Update(AllowedDevice device);
}
