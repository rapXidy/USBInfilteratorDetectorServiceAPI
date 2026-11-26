using System.Net;
using System.Reflection;
using Microsoft.Extensions.Options;
using USBInfilteratorDetectorServiceAPI.Services.Abstractions;
using USBInfilteratorDetectorServiceAPI.Services.Options;

namespace USBInfilteratorDetectorServiceAPI.Infrastructure.Notifications;

/// <summary>
/// Loads HTML email templates from the output Templates folder and substitutes tokens.
/// </summary>
public sealed class EmailTemplateRenderer : IEmailTemplateRenderer
{
    private readonly string _templatesRoot;
    private readonly string _portalUrl;

    public EmailTemplateRenderer(IOptions<PortalOptions> portalOptions)
    {
        var baseDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
                      ?? AppContext.BaseDirectory;
        _templatesRoot = Path.Combine(baseDir, "Notifications", "Templates");
        _portalUrl = string.IsNullOrWhiteSpace(portalOptions.Value.PortalUrl)
            ? "http://localhost:5173/login"
            : portalOptions.Value.PortalUrl.Trim();
    }

    /// <inheritdoc />
    public string Render(string templateName, IReadOnlyDictionary<string, string> tokens)
    {
        var layout = ReadTemplate("_Layout.html");
        var content = ReadTemplate($"{templateName}.html");

        var merged = new Dictionary<string, string>(tokens, StringComparer.OrdinalIgnoreCase)
        {
            ["PortalUrl"] = _portalUrl
        };

        var encoded = merged.ToDictionary(
            x => x.Key,
            x => WebUtility.HtmlEncode(x.Value ?? string.Empty),
            StringComparer.OrdinalIgnoreCase);

        // Content may intentionally include markup from nested templates; tokens inside content are encoded.
        foreach (var pair in encoded)
            content = content.Replace("{{" + pair.Key + "}}", pair.Value, StringComparison.OrdinalIgnoreCase);

        var heading = encoded.GetValueOrDefault("Heading", encoded.GetValueOrDefault("Subject", "Notification"));
        var subject = encoded.GetValueOrDefault("Subject", heading);

        var html = layout
            .Replace("{{Subject}}", subject, StringComparison.OrdinalIgnoreCase)
            .Replace("{{Heading}}", heading, StringComparison.OrdinalIgnoreCase)
            .Replace("{{Content}}", content, StringComparison.OrdinalIgnoreCase);

        // Allow shared layout tokens (e.g. PortalUrl CTA) after content is merged.
        foreach (var pair in encoded)
            html = html.Replace("{{" + pair.Key + "}}", pair.Value, StringComparison.OrdinalIgnoreCase);

        return html;
    }

    private string ReadTemplate(string fileName)
    {
        var path = Path.Combine(_templatesRoot, fileName);
        if (!File.Exists(path))
            throw new FileNotFoundException($"Email template not found: {fileName}", path);
        return File.ReadAllText(path);
    }
}
