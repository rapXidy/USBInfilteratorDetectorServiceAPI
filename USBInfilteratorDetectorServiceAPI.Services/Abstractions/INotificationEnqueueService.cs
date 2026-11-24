using USBInfilteratorDetectorServiceAPI.Domain.Entities;

namespace USBInfilteratorDetectorServiceAPI.Services.Abstractions;

/// <summary>
/// Enqueues email notifications as Pending NotificationsLog rows for background delivery.
/// </summary>
public interface INotificationEnqueueService
{
    /// <summary>
    /// Enqueues onboarding credentials email to the organization contact.
    /// </summary>
    Task EnqueueOnboardingCredentialsAsync(
        Organization organization,
        string corpCode,
        string apiKey,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Enqueues welcome email for a newly created auditor.
    /// </summary>
    Task EnqueueAuditorWelcomeAsync(
        Organization organization,
        Auditor auditor,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Enqueues high-risk / incomplete-detection alerts to auditor emails or org fallback.
    /// </summary>
    Task EnqueueHighRiskSessionAsync(
        Organization organization,
        UsbSession session,
        Host host,
        bool incompleteDetection,
        IReadOnlyList<Auditor> auditors,
        CancellationToken cancellationToken = default);
}