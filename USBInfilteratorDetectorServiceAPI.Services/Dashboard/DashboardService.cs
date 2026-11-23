using Microsoft.Extensions.Options;
using USBInfilteratorDetectorServiceAPI.Domain.Interfaces;
using USBInfilteratorDetectorServiceAPI.Services.DTOs;
using USBInfilteratorDetectorServiceAPI.Services.Options;

namespace USBInfilteratorDetectorServiceAPI.Services.Dashboard;

/// <summary>
/// Aggregates dashboard summary counts for the tenant home screen.
/// </summary>
public class DashboardService
{
    private readonly IHostRepository _hosts;
    private readonly IUsbSessionRepository _sessions;
    private readonly IAllowedDeviceRepository _devices;
    private readonly ISubscriptionRepository _subscriptions;
    private readonly NotificationOptions _options;

    public DashboardService(
        IHostRepository hosts,
        IUsbSessionRepository sessions,
        IAllowedDeviceRepository devices,
        ISubscriptionRepository subscriptions,
        IOptions<NotificationOptions> options)
    {
        _hosts = hosts;
        _sessions = sessions;
        _devices = devices;
        _subscriptions = subscriptions;
        _options = options.Value;
    }

    /// <summary>
    /// Builds aggregated counts for the dashboard.
    /// </summary>
    public async Task<DashboardSummaryResponse> GetSummaryAsync(Guid organizationId, CancellationToken cancellationToken = default)
    {
        var hostCount = await _hosts.CountByOrganizationAsync(organizationId, cancellationToken);
        var sessionCount = await _sessions.CountAsync(organizationId, cancellationToken);
        var highRisk = await _sessions.CountHighRiskAsync(organizationId, _options.HighRiskThreshold, cancellationToken);
        var devices = await _devices.ListByOrganizationAsync(organizationId, includeInactive: false, cancellationToken);
        var subscription = await _subscriptions.GetCurrentByOrganizationIdAsync(organizationId, cancellationToken);

        return new DashboardSummaryResponse(
            hostCount,
            sessionCount,
            highRisk,
            devices.Count,
            subscription?.Plan.ToString() ?? "None",
            subscription?.Status.ToString() ?? "None");
    }
}
