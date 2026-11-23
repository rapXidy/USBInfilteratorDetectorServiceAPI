using System.Text.Json;
using Microsoft.Extensions.Options;
using USBInfilteratorDetectorServiceAPI.Domain.Interfaces;
using USBInfilteratorDetectorServiceAPI.Services.DTOs;
using USBInfilteratorDetectorServiceAPI.Services.Options;

namespace USBInfilteratorDetectorServiceAPI.Services.Sessions;

/// <summary>
/// Returns recent high-severity USB session alerts for a tenant.
/// </summary>
public class AlertService
{
    private readonly IUsbSessionRepository _sessions;
    private readonly NotificationOptions _options;

    public AlertService(IUsbSessionRepository sessions, IOptions<NotificationOptions> options)
    {
        _sessions = sessions;
        _options = options.Value;
    }

    /// <summary>
    /// Lists recent high-risk sessions as alerts.
    /// </summary>
    public async Task<IReadOnlyList<AlertResponse>> GetRecentAsync(Guid organizationId, int take = 50, CancellationToken cancellationToken = default)
    {
        take = Math.Clamp(take, 1, 200);
        var sessions = await _sessions.GetHighRiskRecentAsync(organizationId, _options.HighRiskThreshold, take, cancellationToken);

        return sessions.Select(s => new AlertResponse(
            s.SessionId,
            s.Id,
            s.Host.HostName,
            s.DeviceSerial,
            s.RiskScore,
            s.DetectionComplete,
            s.ConnectedAt,
            s.Reasons
                .OrderByDescending(r => r.Weight)
                .Take(3)
                .Select(r => new SessionReasonResponse(
                    r.RuleName,
                    r.Weight,
                    r.Description,
                    JsonSerializer.Deserialize<List<long>>(r.EventRecordIdsJson) ?? new List<long>()))
                .ToList())).ToList();
    }
}
