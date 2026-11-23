using Microsoft.EntityFrameworkCore;
using USBInfilteratorDetectorServiceAPI.Domain.Entities;
using USBInfilteratorDetectorServiceAPI.Domain.Interfaces;

namespace USBInfilteratorDetectorServiceAPI.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF Core repository for USB sessions.
/// </summary>
public class UsbSessionRepository : IUsbSessionRepository
{
    private readonly AppDbContext _db;

    public UsbSessionRepository(AppDbContext db) => _db = db;

    /// <inheritdoc />
    public async Task AddAsync(UsbSession session, CancellationToken cancellationToken = default)
        => await _db.UsbSessions.AddAsync(session, cancellationToken);

    /// <inheritdoc />
    public Task<UsbSession?> GetByIdAsync(Guid organizationId, Guid id, CancellationToken cancellationToken = default)
        => _db.UsbSessions
            .Include(x => x.Host)
            .Include(x => x.Reasons)
            .Include(x => x.Events)
            .FirstOrDefaultAsync(x => x.OrganizationId == organizationId && x.Id == id, cancellationToken);

    /// <inheritdoc />
    public Task<UsbSession?> GetBySessionIdAsync(Guid organizationId, Guid sessionId, CancellationToken cancellationToken = default)
        => _db.UsbSessions
            .Include(x => x.Reasons)
            .Include(x => x.Events)
            .FirstOrDefaultAsync(x => x.OrganizationId == organizationId && x.SessionId == sessionId, cancellationToken);

    /// <inheritdoc />
    public async Task<IReadOnlyList<UsbSession>> QueryAsync(
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
        var query = _db.UsbSessions
            .Include(x => x.Host)
            .Where(x => x.OrganizationId == organizationId);

        if (fromUtc.HasValue)
            query = query.Where(x => x.ConnectedAt >= fromUtc.Value);
        if (toUtc.HasValue)
            query = query.Where(x => x.ConnectedAt <= toUtc.Value);
        if (minRiskScore.HasValue)
            query = query.Where(x => x.RiskScore >= minRiskScore.Value);
        if (!string.IsNullOrWhiteSpace(hostName))
            query = query.Where(x => x.Host.HostName.Contains(hostName));
        if (!string.IsNullOrWhiteSpace(deviceSerial))
            query = query.Where(x => x.DeviceSerial == deviceSerial);
        if (detectionComplete.HasValue)
            query = query.Where(x => x.DetectionComplete == detectionComplete.Value);

        return await query
            .OrderByDescending(x => x.ConnectedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<UsbSession>> GetHighRiskRecentAsync(Guid organizationId, int minRiskScore, int take, CancellationToken cancellationToken = default)
        => await _db.UsbSessions
            .Include(x => x.Host)
            .Include(x => x.Reasons)
            .Where(x => x.OrganizationId == organizationId && x.RiskScore >= minRiskScore)
            .OrderByDescending(x => x.ConnectedAt)
            .Take(take)
            .ToListAsync(cancellationToken);

    /// <inheritdoc />
    public Task<int> CountAsync(Guid organizationId, CancellationToken cancellationToken = default)
        => _db.UsbSessions.CountAsync(x => x.OrganizationId == organizationId, cancellationToken);

    /// <inheritdoc />
    public Task<int> CountHighRiskAsync(Guid organizationId, int minRiskScore, CancellationToken cancellationToken = default)
        => _db.UsbSessions.CountAsync(x => x.OrganizationId == organizationId && x.RiskScore >= minRiskScore, cancellationToken);
}
