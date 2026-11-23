using USBInfilteratorDetectorServiceAPI.Domain.Enums;
using USBInfilteratorDetectorServiceAPI.Domain.Interfaces;
using USBInfilteratorDetectorServiceAPI.Services.Common;
using USBInfilteratorDetectorServiceAPI.Services.DTOs;

namespace USBInfilteratorDetectorServiceAPI.Services.Subscriptions;

/// <summary>
/// Returns subscription status and basic feature-flag limits.
/// </summary>
public class SubscriptionService
{
    private readonly ISubscriptionRepository _subscriptions;

    public SubscriptionService(ISubscriptionRepository subscriptions)
    {
        _subscriptions = subscriptions;
    }

    /// <summary>
    /// Gets the current subscription for the organization.
    /// </summary>
    public async Task<SubscriptionResponse> GetCurrentAsync(Guid organizationId, CancellationToken cancellationToken = default)
    {
        var subscription = await _subscriptions.GetCurrentByOrganizationIdAsync(organizationId, cancellationToken)
            ?? throw new ServiceException("Subscription not found.", 404);

        var (maxHosts, webhooks) = FeatureFlags(subscription.Plan);
        return new SubscriptionResponse(
            subscription.Id,
            subscription.Plan,
            subscription.Status,
            subscription.StartsAt,
            subscription.EndsAt,
            maxHosts,
            webhooks);
    }

    /// <summary>
    /// Evaluates simple plan feature flags.
    /// </summary>
    public static (int MaxHosts, bool WebhooksEnabled) FeatureFlags(SubscriptionPlan plan)
        => plan switch
        {
            SubscriptionPlan.FreeTrial => (5, false),
            SubscriptionPlan.Pro => (50, true),
            SubscriptionPlan.Enterprise => (int.MaxValue, true),
            _ => (5, false)
        };
}
