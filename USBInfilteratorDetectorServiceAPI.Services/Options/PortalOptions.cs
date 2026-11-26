namespace USBInfilteratorDetectorServiceAPI.Services.Options;

/// <summary>
/// Frontend portal URL used in outbound email call-to-action links.
/// Bound from the root-level PortalUrl configuration key.
/// </summary>
public sealed class PortalOptions
{
    public const string PortalUrlKey = "PortalUrl";

    /// <summary>
    /// Absolute URL of the auditor portal (typically the login page).
    /// </summary>
    public string PortalUrl { get; set; } = "http://localhost:5173/login";
}
