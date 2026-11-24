using USBInfilteratorDetectorServiceAPI.Domain.Entities;
using USBInfilteratorDetectorServiceAPI.Domain.Enums;
using USBInfilteratorDetectorServiceAPI.Domain.Interfaces;
using USBInfilteratorDetectorServiceAPI.Services.Abstractions;

namespace USBInfilteratorDetectorServiceAPI.Services.Notifications;

/// <summary>
/// Builds HTML notification bodies and persists Pending NotificationsLog rows.
/// </summary>
public sealed class NotificationEnqueueService : INotificationEnqueueService
{
    private readonly INotificationsLogRepository _notifications;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailTemplateRenderer _templates;

    public NotificationEnqueueService(
        INotificationsLogRepository notifications,
        IUnitOfWork unitOfWork,
        IEmailTemplateRenderer templates)
    {
        _notifications = notifications;
        _unitOfWork = unitOfWork;
        _templates = templates;
    }

    /// <inheritdoc />
    public async Task EnqueueOnboardingCredentialsAsync(
        Organization organization,
        string corpCode,
        string apiKey,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(organization.Email))
            return;

        var completedAt = organization.OnboardingCompletedAt ?? DateTime.UtcNow;
        var prefix = apiKey.Length >= 8 ? apiKey[..8] : apiKey;
        var subject = $"Onboarding complete — {organization.Name}";
        var body = _templates.Render("OnboardingCredentials", new Dictionary<string, string>
        {
            ["Subject"] = subject,
            ["Heading"] = "Organization credentials",
            ["OrganizationName"] = organization.Name,
            ["CorpCode"] = corpCode,
            ["ApiKey"] = apiKey,
            ["ApiKeyPrefix"] = prefix,
            ["CompletedAtUtc"] = completedAt.ToString("u")
        });

        await AddPendingAsync(
            organization.Id,
            usbSessionId: null,
            severity: "Info",
            title: subject,
            body: body,
            destinationEmail: organization.Email.Trim(),
            cancellationToken).ConfigureAwait(false);

        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task EnqueueAuditorWelcomeAsync(
        Organization organization,
        Auditor auditor,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(auditor.Email))
            return;

        var subject = $"Auditor access — {organization.Name}";
        var body = _templates.Render("AuditorWelcome", new Dictionary<string, string>
        {
            ["Subject"] = subject,
            ["Heading"] = "Welcome to the auditor portal",
            ["OrganizationName"] = organization.Name,
            ["DisplayName"] = auditor.DisplayName,
            ["Username"] = auditor.Username,
            ["Role"] = auditor.Role.ToString(),
            ["CorpCode"] = organization.CorpCode ?? "Pending completion"
        });

        await AddPendingAsync(
            organization.Id,
            usbSessionId: null,
            severity: "Info",
            title: subject,
            body: body,
            destinationEmail: auditor.Email.Trim(),
            cancellationToken).ConfigureAwait(false);

        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task EnqueueHighRiskSessionAsync(
        Organization organization,
        UsbSession session,
        Host host,
        bool incompleteDetection,
        IReadOnlyList<Auditor> auditors,
        CancellationToken cancellationToken = default)
    {
        var recipients = auditors
            .Where(a => a.IsActive && !string.IsNullOrWhiteSpace(a.Email))
            .Select(a => a.Email!.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (recipients.Count == 0 && !string.IsNullOrWhiteSpace(organization.Email))
            recipients.Add(organization.Email.Trim());

        if (recipients.Count == 0)
            return;

        var title = incompleteDetection
            ? "Detection incomplete – required audit settings are disabled"
            : $"High-risk USB session (score {session.RiskScore})";

        var intro = incompleteDetection
            ? "A USB session was recorded while required Windows audit settings were incomplete. File-level detection may be partial."
            : "A USB session exceeded your organization's high-risk threshold and requires review.";

        var body = _templates.Render("HighRiskUsbSession", new Dictionary<string, string>
        {
            ["Subject"] = title,
            ["Heading"] = incompleteDetection ? "Detection incomplete" : "High-risk USB activity",
            ["IntroText"] = intro,
            ["OrganizationName"] = organization.Name,
            ["Severity"] = "High",
            ["HostName"] = host.HostName,
            ["LoggedInUser"] = host.LoggedInUser ?? "—",
            ["DeviceSerial"] = session.DeviceSerial,
            ["RiskScore"] = session.RiskScore.ToString(),
            ["SessionId"] = session.Id.ToString(),
            ["ConnectedAtUtc"] = session.ConnectedAt.ToString("u")
        });

        foreach (var email in recipients)
        {
            await AddPendingAsync(
                organization.Id,
                session.Id,
                "High",
                title,
                body,
                email,
                cancellationToken).ConfigureAwait(false);
        }
        // Caller (ingest) persists session + notifications in one SaveChanges.
    }

    private async Task AddPendingAsync(
        Guid organizationId,
        Guid? usbSessionId,
        string severity,
        string title,
        string body,
        string destinationEmail,
        CancellationToken cancellationToken)
    {
        await _notifications.AddAsync(new NotificationsLog
        {
            Id = Guid.NewGuid(),
            OrganizationId = organizationId,
            UsbSessionId = usbSessionId,
            Severity = severity,
            Title = title,
            Body = body,
            Channel = NotificationChannel.Email,
            Status = NotificationStatus.Pending,
            DestinationEmail = destinationEmail,
            RetryCount = 0,
            CreatedAt = DateTime.UtcNow
        }, cancellationToken).ConfigureAwait(false);
    }
}
