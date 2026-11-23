using System.Text.Json;
using USBInfilteratorDetectorServiceAPI.Domain.Interfaces;
using USBInfilteratorDetectorServiceAPI.Services.Common;
using USBInfilteratorDetectorServiceAPI.Services.DTOs;

namespace USBInfilteratorDetectorServiceAPI.Services.Sessions;

/// <summary>
/// Queries USB sessions for the authenticated tenant.
/// </summary>
public class SessionService
{
    private readonly IUsbSessionRepository _sessions;

    public SessionService(IUsbSessionRepository sessions)
    {
        _sessions = sessions;
    }

    /// <summary>
    /// Lists sessions with optional filters.
    /// </summary>
    public async Task<IReadOnlyList<SessionListItemResponse>> QueryAsync(
        Guid organizationId,
        DateTime? fromUtc,
        DateTime? toUtc,
        int? minRiskScore,
        string? hostName,
        string? deviceSerial,
        bool? detectionComplete,
        int skip,
        int take,
        CancellationToken cancellationToken = default)
    {
        take = Math.Clamp(take <= 0 ? 50 : take, 1, 200);
        skip = Math.Max(0, skip);

        var sessions = await _sessions.QueryAsync(
            organizationId, fromUtc, toUtc, minRiskScore, hostName, deviceSerial, detectionComplete, skip, take, cancellationToken);

        return sessions.Select(s => new SessionListItemResponse(
            s.Id,
            s.SessionId,
            s.Host.HostName,
            s.DeviceSerial,
            s.Vendor,
            s.Model,
            s.ConnectedAt,
            s.DisconnectedAt,
            s.TotalBytesWritten,
            s.RiskScore,
            s.DetectionComplete)).ToList();
    }

    /// <summary>
    /// Returns full session detail including reasons and events.
    /// </summary>
    public async Task<SessionDetailResponse> GetByIdAsync(Guid organizationId, Guid id, CancellationToken cancellationToken = default)
    {
        var session = await _sessions.GetByIdAsync(organizationId, id, cancellationToken)
            ?? throw new ServiceException("Session not found.", 404);

        return new SessionDetailResponse(
            session.Id,
            session.SessionId,
            session.Host.HostName,
            session.Host.LoggedInUser,
            session.DeviceSerial,
            session.Vendor,
            session.Model,
            session.ConnectedAt,
            session.DisconnectedAt,
            session.TotalBytesWritten,
            session.RiskScore,
            session.DetectionComplete,
            session.Reasons.Select(r => new SessionReasonResponse(
                r.RuleName,
                r.Weight,
                r.Description,
                JsonSerializer.Deserialize<List<long>>(r.EventRecordIdsJson) ?? new List<long>())).ToList(),
            session.Events.Select(e => new RawEventResponse(
                e.EventId, e.RecordId, e.TimeCreated, e.Activity, e.Description)).ToList());
    }
}
