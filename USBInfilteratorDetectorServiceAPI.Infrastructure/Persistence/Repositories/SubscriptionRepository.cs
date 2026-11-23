using Microsoft.EntityFrameworkCore;
using USBInfilteratorDetectorServiceAPI.Domain.Entities;
using USBInfilteratorDetectorServiceAPI.Domain.Enums;
using USBInfilteratorDetectorServiceAPI.Domain.Interfaces;

namespace USBInfilteratorDetectorServiceAPI.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF Core repository for subscriptions.
/// </summary>
public class SubscriptionRepository : ISubscriptionRepository
{
    private readonly AppDbContext _db;

    public SubscriptionRepository(AppDbContext db) => _db = db;

    /// <inheritdoc />
    public async Task AddAsync(Subscription subscription, CancellationToken cancellationToken = default)
        => await _db.Subscriptions.AddAsync(subscription, cancellationToken);

    /// <inheritdoc />
    public Task<Subscription?> GetCurrentByOrganizationIdAsync(Guid organizationId, CancellationToken cancellationToken = default)
        => _db.Subscriptions
            .Where(x => x.OrganizationId == organizationId)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
}
