namespace USBInfilteratorDetectorServiceAPI.Services.Options;

/// <summary>
/// JWT signing and lifetime configuration.
/// </summary>
public class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = "USBInfilteratorDetectorServiceAPI";
    public string Audience { get; set; } = "USBInfilteratorDetectorClients";
    public string SigningKey { get; set; } = string.Empty;
    public int AccessTokenMinutes { get; set; } = 15;
    public int RefreshTokenDays { get; set; } = 7;
}
