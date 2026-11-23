namespace USBInfilteratorDetectorServiceAPI.Services.DTOs;

public record DashboardSummaryResponse(
    int HostCount,
    int SessionCount,
    int HighRiskSessionCount,
    int AllowedDeviceCount,
    string SubscriptionPlan,
    string SubscriptionStatus);
