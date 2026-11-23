using USBInfilteratorDetectorServiceAPI.Domain.Enums;

namespace USBInfilteratorDetectorServiceAPI.Services.DTOs;

public record SubscriptionResponse(
    Guid Id,
    SubscriptionPlan Plan,
    SubscriptionStatus Status,
    DateTime StartsAt,
    DateTime EndsAt,
    int MaxHosts,
    bool WebhooksEnabled);
