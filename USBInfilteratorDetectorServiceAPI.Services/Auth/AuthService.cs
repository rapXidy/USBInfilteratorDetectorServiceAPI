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
using USBInfilteratorDetectorServiceAPI.Services.Security;

namespace USBInfilteratorDetectorServiceAPI.Services.Auth;

/// <summary>
/// Handles tenant auditor authentication, refresh, logout, and password flows.
/// </summary>
public class AuthService
{
    private const string ForgotPasswordGenericMessage =
        "If an account matches those details and has an email on file, a temporary password has been sent.";

    private readonly IOrganizationRepository _organizations;
    private readonly IAuditorRepository _auditors;
    private readonly IRefreshTokenRepository _refreshTokens;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly INotificationEnqueueService _notifications;
    private readonly JwtOptions _jwtOptions;

    public AuthService(
        IOrganizationRepository organizations,
        IAuditorRepository auditors,
        IRefreshTokenRepository refreshTokens,
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService,
        INotificationEnqueueService notifications,
        IOptions<JwtOptions> jwtOptions)
    {
        _organizations = organizations;
        _auditors = auditors;
        _refreshTokens = refreshTokens;
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
        _notifications = notifications;
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
    /// Issues a temporary password by email when possible; always returns a generic success message.
    /// </summary>
    public async Task<ForgotPasswordResponse> ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.CorpCode) || string.IsNullOrWhiteSpace(request.Username))
            return new ForgotPasswordResponse(ForgotPasswordGenericMessage);

        var organization = await _organizations.GetByCorpCodeAsync(request.CorpCode.Trim(), cancellationToken);
        if (organization is null || !organization.IsActive || organization.OnboardingStatus != OnboardingStatus.Completed)
            return new ForgotPasswordResponse(ForgotPasswordGenericMessage);

        var auditor = await _auditors.GetByUsernameAsync(organization.Id, request.Username.Trim(), cancellationToken);
        if (auditor is null || !auditor.IsActive || string.IsNullOrWhiteSpace(auditor.Email))
            return new ForgotPasswordResponse(ForgotPasswordGenericMessage);

        var temporaryPassword = TemporaryPasswordGenerator.Generate();
        auditor.PasswordHash = _passwordHasher.Hash(temporaryPassword);
        auditor.UpdatedAt = DateTime.UtcNow;
        _auditors.Update(auditor);
        await _refreshTokens.RevokeAllForAuditorAsync(auditor.Id, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _notifications.EnqueuePasswordResetAsync(organization, auditor, temporaryPassword, cancellationToken);

        return new ForgotPasswordResponse(ForgotPasswordGenericMessage);
    }

    /// <summary>
    /// Changes the authenticated auditor's password and revokes existing refresh tokens.
    /// </summary>
    public async Task ChangePasswordAsync(Guid organizationId, Guid auditorId, ChangePasswordRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.CurrentPassword) || string.IsNullOrWhiteSpace(request.NewPassword))
            throw new ServiceException("Current and new passwords are required.");

        if (request.NewPassword.Length < 8)
            throw new ServiceException("New password must be at least 8 characters.");

        var auditor = await _auditors.GetByIdAsync(organizationId, auditorId, cancellationToken)
            ?? throw new ServiceException("Auditor not found.", 404);

        if (!auditor.IsActive)
            throw new ServiceException("Account is inactive.", 403);

        if (!_passwordHasher.Verify(request.CurrentPassword, auditor.PasswordHash))
            throw new ServiceException("Current password is incorrect.", 400);

        auditor.PasswordHash = _passwordHasher.Hash(request.NewPassword);
        auditor.UpdatedAt = DateTime.UtcNow;
        _auditors.Update(auditor);
        await _refreshTokens.RevokeAllForAuditorAsync(auditor.Id, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
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
