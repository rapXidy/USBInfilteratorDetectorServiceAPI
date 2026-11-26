using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using USBInfilteratorDetectorServiceAPI.Domain.Entities;
using USBInfilteratorDetectorServiceAPI.Domain.Enums;
using USBInfilteratorDetectorServiceAPI.Domain.Interfaces;
using USBInfilteratorDetectorServiceAPI.Services.Abstractions;
using USBInfilteratorDetectorServiceAPI.Services.Options;

namespace USBInfilteratorDetectorServiceAPI.Infrastructure.Notifications;

/// <summary>
/// Background worker that dispatches Pending email notifications with configurable retries.
/// </summary>
public sealed class NotificationBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IOptionsMonitor<NotificationOptions> _options;
    private readonly ILogger<NotificationBackgroundService> _logger;

    public NotificationBackgroundService(
        IServiceScopeFactory scopeFactory,
        IOptionsMonitor<NotificationOptions> options,
        ILogger<NotificationBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options;
        _logger = logger;
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "NotificationBackgroundService started. Polling every {Seconds}s; MaxAttempts={MaxAttempts}; BatchSize={BatchSize}",
            Math.Max(5, _options.CurrentValue.Dispatch.PollingIntervalSeconds),
            Math.Max(1, _options.CurrentValue.Dispatch.MaxAttempts),
            Math.Max(1, _options.CurrentValue.Dispatch.BatchSize));

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessPendingEmailNotificationsAsync(stoppingToken).ConfigureAwait(false);
            }
            catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogError(ex, "Unhandled error during notification processing cycle");
            }

            var delaySeconds = Math.Max(5, _options.CurrentValue.Dispatch.PollingIntervalSeconds);
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(delaySeconds), stoppingToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }

        _logger.LogInformation("NotificationBackgroundService stopped");
    }

    private async Task ProcessPendingEmailNotificationsAsync(CancellationToken stoppingToken)
    {
        var dispatch = _options.CurrentValue.Dispatch;
        var maxAttempts = Math.Max(1, dispatch.MaxAttempts);
        var batchSize = Math.Max(1, dispatch.BatchSize);

        using var scope = _scopeFactory.CreateScope();
        var notifications = scope.ServiceProvider.GetRequiredService<INotificationsLogRepository>();
        var emailSender = scope.ServiceProvider.GetRequiredService<IEmailNotificationSender>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var pending = await notifications
            .GetPendingForDispatchAsync(batchSize, maxAttempts, stoppingToken)
            .ConfigureAwait(false);

        if (pending.Count == 0)
        {
            _logger.LogDebug("No pending email notifications found");
            return;
        }

        _logger.LogInformation("Processing {Count} pending email notification(s)", pending.Count);

        foreach (var notification in pending)
        {
            if (stoppingToken.IsCancellationRequested)
                break;

            await ProcessSingleNotificationAsync(
                notification,
                notifications,
                emailSender,
                unitOfWork,
                maxAttempts,
                stoppingToken).ConfigureAwait(false);
        }
    }

    private async Task ProcessSingleNotificationAsync(
        NotificationsLog notification,
        INotificationsLogRepository notifications,
        IEmailNotificationSender emailSender,
        IUnitOfWork unitOfWork,
        int maxAttempts,
        CancellationToken stoppingToken)
    {
        try
        {
            notification.RetryCount++;
            notification.LastAttemptedAt = DateTime.UtcNow;
            notifications.Update(notification);
            await unitOfWork.SaveChangesAsync(stoppingToken).ConfigureAwait(false);

            _logger.LogInformation(
                "Sending notification {NotificationId} to {Recipient} (Attempt {Attempt}/{MaxAttempts})",
                notification.Id,
                notification.DestinationEmail,
                notification.RetryCount,
                maxAttempts);

            var (success, error) = await emailSender
                .SendAsync(
                    notification.DestinationEmail,
                    notification.Title,
                    notification.Body,
                    isHtml: true,
                    stoppingToken)
                .ConfigureAwait(false);

            if (success)
            {
                notification.Status = NotificationStatus.Sent;
                notification.SentAt = DateTime.UtcNow;
                notification.ErrorMessage = null;
                _logger.LogInformation(
                    "Successfully sent notification {NotificationId} to {Recipient}",
                    notification.Id,
                    notification.DestinationEmail);
            }
            else
            {
                MarkAsFailedOrRetry(notification, error ?? "Unknown email delivery failure", maxAttempts);
                _logger.LogWarning(
                    "Failed to send notification {NotificationId} to {Recipient}. Reason: {Reason}. RetryCount: {RetryCount}",
                    notification.Id,
                    notification.DestinationEmail,
                    error,
                    notification.RetryCount);
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            MarkAsFailedOrRetry(notification, ex.Message, maxAttempts);
            _logger.LogError(
                ex,
                "Exception sending notification {NotificationId} to {Recipient}. RetryCount: {RetryCount}",
                notification.Id,
                notification.DestinationEmail,
                notification.RetryCount);
        }
        finally
        {
            try
            {
                notifications.Update(notification);
                await unitOfWork.SaveChangesAsync(stoppingToken).ConfigureAwait(false);
            }
            catch (Exception updateEx)
            {
                _logger.LogError(
                    updateEx,
                    "Failed to persist status update for notification {NotificationId}",
                    notification.Id);
            }
        }
    }

    private static void MarkAsFailedOrRetry(NotificationsLog notification, string reason, int maxAttempts)
    {
        notification.ErrorMessage = reason.Length > 1024 ? reason[..1024] : reason;
        notification.Status = notification.RetryCount >= maxAttempts
            ? NotificationStatus.Failed
            : NotificationStatus.Pending;
    }
}
