using System.Text.Json;
using Microsoft.Extensions.Options;
using USBInfilteratorDetectorServiceAPI.Domain.Entities;
using USBInfilteratorDetectorServiceAPI.Domain.Enums;
using USBInfilteratorDetectorServiceAPI.Domain.Interfaces;
using USBInfilteratorDetectorServiceAPI.Services.Abstractions;
using USBInfilteratorDetectorServiceAPI.Services.Common;
using USBInfilteratorDetectorServiceAPI.Services.DTOs;
using USBInfilteratorDetectorServiceAPI.Services.Options;

namespace USBInfilteratorDetectorServiceAPI.Services.Ingest;

/// <summary>
/// Persists Worker Service batch payloads under the authenticated organization.
/// </summary>
public class IngestService
{
    private readonly IOrganizationRepository _organizations;
    private readonly IOrganizationApiCredentialRepository _credentials;
    private readonly IHostRepository _hosts;
    private readonly IUsbSessionRepository _sessions;
    private readonly INotificationsLogRepository _notifications;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IEmailNotificationSender _emailSender;
    private readonly NotificationOptions _notificationOptions;

    public IngestService(
        IOrganizationRepository organizations,
        IOrganizationApiCredentialRepository credentials,
        IHostRepository hosts,
        IUsbSessionRepository sessions,
        INotificationsLogRepository notifications,
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        IEmailNotificationSender emailSender,
        IOptions<NotificationOptions> notificationOptions)
    {
        _organizations = organizations;
        _credentials = credentials;
        _hosts = hosts;
        _sessions = sessions;
        _notifications = notifications;
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _emailSender = emailSender;
        _notificationOptions = notificationOptions.Value;
    }

    /// <summary>
    /// Resolves and validates a Worker API key, returning the bound organization.
    /// </summary>
    public async Task<Organization> AuthenticateApiKeyAsync(string apiKey, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(apiKey) || apiKey.Length < 12)
            throw new ServiceException("Invalid API key.", 401);

        var prefix = apiKey[..12];
        var candidates = await _credentials.GetActiveByPrefixAsync(prefix, cancellationToken);
        var match = candidates.FirstOrDefault(x => _passwordHasher.Verify(apiKey, x.ApiKeyHash));
        if (match is null)
            throw new ServiceException("Invalid API key.", 401);

        if (!match.Organization.IsActive || match.Organization.OnboardingStatus != OnboardingStatus.Completed)
            throw new ServiceException("Organization is inactive.", 403);

        return match.Organization;
    }

    /// <summary>
    /// Accepts a batch of Worker payloads for the authenticated organization.
    /// </summary>
    public async Task<IngestBatchResponse> IngestBatchAsync(Guid organizationId, IngestBatchRequest request, CancellationToken cancellationToken = default)
    {
        var organization = await _organizations.GetByIdAsync(organizationId, cancellationToken)
            ?? throw new ServiceException("Organization not found.", 404);

        if (!organization.IsActive)
            throw new ServiceException("Organization is inactive.", 403);

        var accepted = 0;
        var skipped = 0;

        foreach (var item in request.Items)
        {
            if (!string.Equals(item.CorpCode, organization.CorpCode, StringComparison.OrdinalIgnoreCase))
            {
                skipped++;
                continue;
            }

            await PersistPayloadAsync(organization, item, cancellationToken);
            accepted++;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return new IngestBatchResponse(accepted, skipped);
    }

    private async Task PersistPayloadAsync(Organization organization, IngestPayloadDto payload, CancellationToken cancellationToken)
    {
        var host = await _hosts.GetByDeviceIdAsync(organization.Id, payload.Host.DeviceId, cancellationToken);
        if (host is null)
        {
            host = new Host
            {
                Id = Guid.NewGuid(),
                OrganizationId = organization.Id,
                HostName = payload.Host.HostName,
                DeviceId = payload.Host.DeviceId,
                OsVersion = payload.Host.OsVersion,
                LoggedInUser = payload.Host.LoggedInUser,
                LastSeenAt = DateTime.UtcNow
            };
            await _hosts.AddAsync(host, cancellationToken);
        }
        else
        {
            host.HostName = payload.Host.HostName;
            host.OsVersion = payload.Host.OsVersion;
            host.LoggedInUser = payload.Host.LoggedInUser;
            host.LastSeenAt = DateTime.UtcNow;
            _hosts.Update(host);
        }

        var existing = await _sessions.GetBySessionIdAsync(organization.Id, payload.Session.SessionId, cancellationToken);
        if (existing is not null)
        {
            return;
        }

        var session = new UsbSession
        {
            Id = Guid.NewGuid(),
            OrganizationId = organization.Id,
            HostId = host.Id,
            SessionId = payload.Session.SessionId,
            DeviceSerial = payload.Session.DeviceSerial,
            Vendor = payload.Session.Vendor,
            Model = payload.Session.Model,
            ConnectedAt = payload.Session.ConnectedAt,
            DisconnectedAt = payload.Session.DisconnectedAt,
            TotalBytesWritten = payload.Session.TotalBytesWritten,
            RiskScore = payload.Session.RiskScore,
            DetectionComplete = payload.AuditSettingsStatus.DetectionComplete,
            CreatedAt = DateTime.UtcNow,
            Reasons = payload.Session.Reasons.Select(r => new SessionReason
            {
                Id = Guid.NewGuid(),
                RuleName = r.Rule,
                Weight = r.Weight,
                Description = r.Description,
                EventRecordIdsJson = JsonSerializer.Serialize(r.EventRecordIds)
            }).ToList(),
            Events = payload.Events.Select(e => new RawEvent
            {
                Id = Guid.NewGuid(),
                EventId = e.EventId,
                RecordId = e.RecordId,
                TimeCreated = e.TimeCreated,
                Activity = e.Activity,
                Description = e.Description
            }).ToList()
        };

        await _sessions.AddAsync(session, cancellationToken);

        var incompleteDetection = !payload.AuditSettingsStatus.DetectionComplete
            || !payload.AuditSettingsStatus.PnpActivityEnabled
            || !payload.AuditSettingsStatus.RemovableStorageEnabled;

        if (session.RiskScore >= _notificationOptions.HighRiskThreshold || incompleteDetection)
        {
            await QueueNotificationAsync(organization, session, incompleteDetection, cancellationToken);
        }
    }

    private async Task QueueNotificationAsync(Organization organization, UsbSession session, bool incompleteDetection, CancellationToken cancellationToken)
    {
        var title = incompleteDetection
            ? "Detection incomplete – required audit settings are disabled"
            : $"High-risk USB session (score {session.RiskScore})";

        var body =
            $"Organization: {organization.Name}\nHost: {session.HostId}\nDevice: {session.DeviceSerial}\nRiskScore: {session.RiskScore}";

        var entry = new NotificationsLog
        {
            Id = Guid.NewGuid(),
            OrganizationId = organization.Id,
            UsbSessionId = session.Id,
            Severity = incompleteDetection ? "High" : "High",
            Title = title,
            Body = body,
            Channel = NotificationChannel.Email,
            Status = NotificationStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        var (success, error) = await _emailSender.SendAsync(organization.Email, title, body, cancellationToken);
        entry.Status = success ? NotificationStatus.Sent : NotificationStatus.Failed;
        entry.SentAt = success ? DateTime.UtcNow : null;
        entry.ErrorMessage = error;
        await _notifications.AddAsync(entry, cancellationToken);
    }
}
