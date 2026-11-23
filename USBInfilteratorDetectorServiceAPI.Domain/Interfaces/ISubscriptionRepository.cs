using USBInfilteratorDetectorServiceAPI.Domain.Entities;

namespace USBInfilteratorDetectorServiceAPI.Domain.Interfaces;

/// <summary>
/// Persistence operations for subscriptions.
/// </summary>
public interface ISubscriptionRepository
{
    Task AddAsync(Subscription subscription, CancellationToken cancellationToken = default);
    Task<Subscription?> GetCurrentByOrganizationIdAsync(Guid organizationId, CancellationToken cancellationToken = default);
}
