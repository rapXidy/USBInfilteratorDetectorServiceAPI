using Microsoft.EntityFrameworkCore;
using USBInfilteratorDetectorServiceAPI.Domain.Entities;
using USBInfilteratorDetectorServiceAPI.Domain.Enums;
using USBInfilteratorDetectorServiceAPI.Domain.Interfaces;

namespace USBInfilteratorDetectorServiceAPI.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF Core repository for notification logs.
/// </summary>
public class NotificationsLogRepository : INotificationsLogRepository
{
    private readonly AppDbContext _db;

    public NotificationsLogRepository(AppDbContext db) => _db = db;

    /// <inheritdoc />
    public async Task AddAsync(NotificationsLog entry, CancellationToken cancellationToken = default)
        => await _db.NotificationsLogs.AddAsync(entry, cancellationToken);

    /// <inheritdoc />
    public async Task<IReadOnlyList<NotificationsLog>> ListRecentAsync(Guid organizationId, int take, CancellationToken cancellationToken = default)
        => await _db.NotificationsLogs
            .Where(x => x.OrganizationId == organizationId)
            .OrderByDescending(x => x.CreatedAt)
            .Take(take)
            .ToListAsync(cancellationToken);

    /// <inheritdoc />
    public async Task<IReadOnlyList<NotificationsLog>> GetPendingForDispatchAsync(
        int batchSize,
        int maxAttempts,
        CancellationToken cancellationToken = default)
        => await _db.NotificationsLogs
            .Where(x =>
                x.Channel == NotificationChannel.Email &&
                x.Status == NotificationStatus.Pending &&
                x.RetryCount < maxAttempts)
            .OrderBy(x => x.CreatedAt)
            .Take(batchSize)
            .ToListAsync(cancellationToken);

    /// <inheritdoc />
    public void Update(NotificationsLog entry) => _db.NotificationsLogs.Update(entry);
}
