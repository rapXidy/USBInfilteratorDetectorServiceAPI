using USBInfilteratorDetectorServiceAPI.Domain.Entities;

namespace USBInfilteratorDetectorServiceAPI.Domain.Interfaces;

/// <summary>
/// Persistence operations for notification delivery logs.
/// </summary>
public interface INotificationsLogRepository
{
    Task AddAsync(NotificationsLog entry, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<NotificationsLog>> ListRecentAsync(Guid organizationId, int take, CancellationToken cancellationToken = default);
    void Update(NotificationsLog entry);
}
