namespace USBInfilteratorDetectorServiceAPI.Services.Options;

/// <summary>
/// CORS allowed origins for the React frontend.
/// </summary>
public class AppCorsOptions
{
    public const string SectionName = "Cors";
    public string[] AllowedOrigins { get; set; } = Array.Empty<string>();
}
