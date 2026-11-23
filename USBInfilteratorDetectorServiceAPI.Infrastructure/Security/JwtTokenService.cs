using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using USBInfilteratorDetectorServiceAPI.Domain.Entities;
using USBInfilteratorDetectorServiceAPI.Services.Abstractions;
using USBInfilteratorDetectorServiceAPI.Services.Options;

namespace USBInfilteratorDetectorServiceAPI.Infrastructure.Security;

/// <summary>
/// Creates JWT access tokens and refresh-token material.
/// </summary>
public class JwtTokenService : IJwtTokenService
{
    public const string SuperAdminRole = "SuperAdmin";
    public const string ClaimOrganizationId = "organizationId";
    public const string ClaimCorpCode = "corpCode";
    public const string ClaimAuditorId = "auditorId";

    private readonly JwtOptions _options;
    private readonly IPasswordHasher _passwordHasher;

    public JwtTokenService(IOptions<JwtOptions> options, IPasswordHasher passwordHasher)
    {
        _options = options.Value;
        _passwordHasher = passwordHasher;
    }

    /// <inheritdoc />
    public string CreateAuditorAccessToken(Auditor auditor, Organization organization)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, auditor.Id.ToString()),
            new(ClaimTypes.Name, auditor.Username),
            new(ClaimTypes.Role, auditor.Role.ToString()),
            new(ClaimOrganizationId, organization.Id.ToString()),
            new(ClaimCorpCode, organization.CorpCode ?? string.Empty),
            new(ClaimAuditorId, auditor.Id.ToString())
        };
        return CreateToken(claims, TimeSpan.FromMinutes(_options.AccessTokenMinutes));
    }

    /// <inheritdoc />
    public string CreateSuperAdminAccessToken(string username)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, username),
            new(ClaimTypes.Name, username),
            new(ClaimTypes.Role, SuperAdminRole)
        };
        return CreateToken(claims, TimeSpan.FromMinutes(_options.AccessTokenMinutes));
    }

    /// <inheritdoc />
    public string CreateRefreshToken()
        => Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

    /// <inheritdoc />
    public string HashRefreshToken(string refreshToken)
        => _passwordHasher.Hash(refreshToken);

    private string CreateToken(IEnumerable<Claim> claims, TimeSpan lifetime)
    {
        if (string.IsNullOrWhiteSpace(_options.SigningKey) || _options.SigningKey.Length < 32)
            throw new InvalidOperationException("Jwt:SigningKey must be configured and at least 32 characters.");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SigningKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: DateTime.UtcNow.Add(lifetime),
            signingCredentials: credentials);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
