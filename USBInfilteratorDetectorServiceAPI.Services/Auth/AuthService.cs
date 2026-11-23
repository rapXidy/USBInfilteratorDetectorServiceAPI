using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using USBInfilteratorDetectorServiceAPI.Domain.Entities;
using USBInfilteratorDetectorServiceAPI.Domain.Enums;
using USBInfilteratorDetectorServiceAPI.Domain.Interfaces;
using USBInfilteratorDetectorServiceAPI.Services.Abstractions;
using USBInfilteratorDetectorServiceAPI.Services.Common;
using USBInfilteratorDetectorServiceAPI.Services.DTOs;
using USBInfilteratorDetectorServiceAPI.Services.Options;

namespace USBInfilteratorDetectorServiceAPI.Services.Auth;

/// <summary>
/// Handles tenant auditor authentication, refresh, and logout.
/// </summary>
public class AuthService
{
    private readonly IOrganizationRepository _organizations;
    private readonly IAuditorRepository _auditors;
    private readonly IRefreshTokenRepository _refreshTokens;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly JwtOptions _jwtOptions;

    public AuthService(
        IOrganizationRepository organizations,
        IAuditorRepository auditors,
        IRefreshTokenRepository refreshTokens,
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService,
        IOptions<JwtOptions> jwtOptions)
    {
        _organizations = organizations;
        _auditors = auditors;
        _refreshTokens = refreshTokens;
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
        _jwtOptions = jwtOptions.Value;
    }

    /// <summary>
    /// Authenticates an auditor with CorpCode, username, and password.
    /// </summary>
    public async Task<AuthTokenResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var organization = await _organizations.GetByCorpCodeAsync(request.CorpCode.Trim(), cancellationToken)
            ?? throw new ServiceException("Invalid credentials.", 401);

        if (organization.OnboardingStatus != OnboardingStatus.Completed || !organization.IsActive)
            throw new ServiceException("Organization is inactive or onboarding is incomplete.", 403);

        var auditor = await _auditors.GetByUsernameAsync(organization.Id, request.Username.Trim(), cancellationToken)
            ?? throw new ServiceException("Invalid credentials.", 401);

        if (!auditor.IsActive || !_passwordHasher.Verify(request.Password, auditor.PasswordHash))
            throw new ServiceException("Invalid credentials.", 401);

        auditor.LastLoginAt = DateTime.UtcNow;
        _auditors.Update(auditor);
        return await IssueTokensAsync(auditor, organization, cancellationToken);
    }

    /// <summary>
    /// Rotates tokens using a valid refresh token.
    /// </summary>
    public async Task<AuthTokenResponse> RefreshAsync(RefreshRequest request, CancellationToken cancellationToken = default)
    {
        var stored = await _refreshTokens.GetByTokenHashAsync(Sha256(request.RefreshToken), cancellationToken)
            ?? throw new ServiceException("Invalid refresh token.", 401);

        if (stored.RevokedAt.HasValue || stored.ExpiresAt < DateTime.UtcNow)
            throw new ServiceException("Refresh token expired or revoked.", 401);

        var auditor = stored.Auditor;
        var organization = auditor.Organization;
        if (!organization.IsActive || organization.OnboardingStatus != OnboardingStatus.Completed || !auditor.IsActive)
            throw new ServiceException("Account or organization is inactive.", 403);

        stored.RevokedAt = DateTime.UtcNow;
        _refreshTokens.Update(stored);
        return await IssueTokensAsync(auditor, organization, cancellationToken);
    }

    /// <summary>
    /// Revokes a refresh token (logout).
    /// </summary>
    public async Task LogoutAsync(RefreshRequest request, CancellationToken cancellationToken = default)
    {
        var stored = await _refreshTokens.GetByTokenHashAsync(Sha256(request.RefreshToken), cancellationToken);
        if (stored is null)
            return;

        stored.RevokedAt = DateTime.UtcNow;
        _refreshTokens.Update(stored);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task<AuthTokenResponse> IssueTokensAsync(Auditor auditor, Organization organization, CancellationToken cancellationToken)
    {
        var accessToken = _jwtTokenService.CreateAuditorAccessToken(auditor, organization);
        var refreshToken = _jwtTokenService.CreateRefreshToken();
        var expiresAt = DateTime.UtcNow.AddMinutes(_jwtOptions.AccessTokenMinutes);

        await _refreshTokens.AddAsync(new RefreshToken
        {
            Id = Guid.NewGuid(),
            AuditorId = auditor.Id,
            TokenHash = Sha256(refreshToken),
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenDays)
        }, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return new AuthTokenResponse(accessToken, refreshToken, expiresAt);
    }

    private static string Sha256(string value)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(bytes);
    }
}
