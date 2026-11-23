using USBInfilteratorDetectorServiceAPI.Domain.Entities;

namespace USBInfilteratorDetectorServiceAPI.Domain.Interfaces;

/// <summary>
/// Persistence operations for USB sessions.
/// </summary>
public interface IUsbSessionRepository
{
    Task AddAsync(UsbSession session, CancellationToken cancellationToken = default);
    Task<UsbSession?> GetByIdAsync(Guid organizationId, Guid id, CancellationToken cancellationToken = default);
    Task<UsbSession?> GetBySessionIdAsync(Guid organizationId, Guid sessionId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UsbSession>> QueryAsync(
        Guid organizationId,
        DateTime? fromUtc,
        DateTime? toUtc,
        int? minRiskScore,
        string? hostName,
        string? deviceSerial,
        bool? detectionComplete,
        int skip,
        int take,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UsbSession>> GetHighRiskRecentAsync(Guid organizationId, int minRiskScore, int take, CancellationToken cancellationToken = default);
    Task<int> CountAsync(Guid organizationId, CancellationToken cancellationToken = default);
    Task<int> CountHighRiskAsync(Guid organizationId, int minRiskScore, CancellationToken cancellationToken = default);
}
