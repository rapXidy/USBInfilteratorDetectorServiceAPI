namespace USBInfilteratorDetectorServiceAPI.Services.Options;

/// <summary>
/// Gmail/SMTP settings bound from the EmailSettings configuration section.
/// </summary>
public sealed class EmailSettings
{
    public const string SectionName = "EmailSettings";

    public string SmtpServer { get; set; } = "smtp.gmail.com";
    public int Port { get; set; } = 587;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string SenderName { get; set; } = "USB Exfiltration Detector";
    public string SenderEmail { get; set; } = string.Empty;
}
