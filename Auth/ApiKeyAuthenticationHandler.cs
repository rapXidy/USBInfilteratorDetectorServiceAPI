using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using USBInfilteratorDetectorServiceAPI.Infrastructure.Security;
using USBInfilteratorDetectorServiceAPI.Services.Ingest;

namespace USBInfilteratorDetectorServiceAPI.Auth;

/// <summary>
/// Authenticates Worker requests using the X-Api-Key header against per-organization credentials.
/// </summary>
public class ApiKeyAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string SchemeName = "ApiKey";
    public const string HeaderName = "X-Api-Key";

    private readonly IngestService _ingestService;

    public ApiKeyAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IngestService ingestService)
        : base(options, logger, encoder)
    {
        _ingestService = ingestService;
    }

    /// <inheritdoc />
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(HeaderName, out var headerValues))
            return AuthenticateResult.Fail("Missing API key.");

        var apiKey = headerValues.ToString();
        try
        {
            var organization = await _ingestService.AuthenticateApiKeyAsync(apiKey, Context.RequestAborted);
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, organization.CorpCode ?? organization.Id.ToString()),
                new Claim(ClaimTypes.Role, "Worker"),
                new Claim(JwtTokenService.ClaimOrganizationId, organization.Id.ToString()),
                new Claim(JwtTokenService.ClaimCorpCode, organization.CorpCode ?? string.Empty)
            };
            var identity = new ClaimsIdentity(claims, SchemeName);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, SchemeName);
            return AuthenticateResult.Success(ticket);
        }
        catch (Exception ex)
        {
            return AuthenticateResult.Fail(ex.Message);
        }
    }
}
