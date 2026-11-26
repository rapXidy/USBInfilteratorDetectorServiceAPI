using Microsoft.EntityFrameworkCore;
using USBInfilteratorDetectorServiceAPI.Domain.Entities;
using USBInfilteratorDetectorServiceAPI.Domain.Interfaces;

namespace USBInfilteratorDetectorServiceAPI.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF Core repository for refresh tokens.
/// </summary>
public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly AppDbContext _db;

    public RefreshTokenRepository(AppDbContext db) => _db = db;

    /// <inheritdoc />
    public async Task AddAsync(RefreshToken token, CancellationToken cancellationToken = default)
        => await _db.RefreshTokens.AddAsync(token, cancellationToken);

    /// <inheritdoc />
    public Task<RefreshToken?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default)
        => _db.RefreshTokens
            .Include(x => x.Auditor)
            .ThenInclude(x => x.Organization)
            .FirstOrDefaultAsync(x => x.TokenHash == tokenHash, cancellationToken);

    /// <inheritdoc />
    public void Update(RefreshToken token) => _db.RefreshTokens.Update(token);

    /// <inheritdoc />
    public async Task RevokeAllForAuditorAsync(Guid auditorId, CancellationToken cancellationToken = default)
    {
        var tokens = await _db.RefreshTokens
            .Where(x => x.AuditorId == auditorId && x.RevokedAt == null)
            .ToListAsync(cancellationToken);

        var now = DateTime.UtcNow;
        foreach (var token in tokens)
            token.RevokedAt = now;
    }
}
