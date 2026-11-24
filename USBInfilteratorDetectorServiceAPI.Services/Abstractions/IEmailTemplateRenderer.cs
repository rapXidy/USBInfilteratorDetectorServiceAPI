namespace USBInfilteratorDetectorServiceAPI.Services.Abstractions;

/// <summary>
/// Renders reusable HTML email templates with token substitution.
/// </summary>
public interface IEmailTemplateRenderer
{
    /// <summary>
    /// Renders a named content template inside the shared layout.
    /// </summary>
    string Render(string templateName, IReadOnlyDictionary<string, string> tokens);
}
