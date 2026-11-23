using USBInfilteratorDetectorServiceAPI.Domain.Entities;

namespace USBInfilteratorDetectorServiceAPI.Domain.Interfaces;

/// <summary>
/// Persistence operations for monitored hosts.
/// </summary>
public interface IHostRepository
{
    Task AddAsync(Host host, CancellationToken cancellationToken = default);
    Task<Host?> GetByDeviceIdAsync(Guid organizationId, string deviceId, CancellationToken cancellationToken = default);
    Task<int> CountByOrganizationAsync(Guid organizationId, CancellationToken cancellationToken = default);
    void Update(Host host);
}
