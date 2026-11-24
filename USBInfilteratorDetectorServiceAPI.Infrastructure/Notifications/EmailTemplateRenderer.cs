using System.Net;
using System.Reflection;
using USBInfilteratorDetectorServiceAPI.Services.Abstractions;

namespace USBInfilteratorDetectorServiceAPI.Infrastructure.Notifications;

/// <summary>
/// Loads HTML email templates from the output Templates folder and substitutes tokens.
/// </summary>
public sealed class EmailTemplateRenderer : IEmailTemplateRenderer
{
    private readonly string _templatesRoot;

    public EmailTemplateRenderer()
    {
        var baseDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
                      ?? AppContext.BaseDirectory;
        _templatesRoot = Path.Combine(baseDir, "Notifications", "Templates");
    }

    /// <inheritdoc />
    public string Render(string templateName, IReadOnlyDictionary<string, string> tokens)
    {
        var layout = ReadTemplate("_Layout.html");
        var content = ReadTemplate($"{templateName}.html");

        var encoded = tokens.ToDictionary(
            x => x.Key,
            x => WebUtility.HtmlEncode(x.Value ?? string.Empty),
            StringComparer.OrdinalIgnoreCase);

        // Content may intentionally include markup from nested templates; tokens inside content are encoded.
        foreach (var pair in encoded)
            content = content.Replace("{{" + pair.Key + "}}", pair.Value, StringComparison.OrdinalIgnoreCase);

        var heading = encoded.GetValueOrDefault("Heading", encoded.GetValueOrDefault("Subject", "Notification"));
        var subject = encoded.GetValueOrDefault("Subject", heading);

        return layout
            .Replace("{{Subject}}", subject, StringComparison.OrdinalIgnoreCase)
            .Replace("{{Heading}}", heading, StringComparison.OrdinalIgnoreCase)
            .Replace("{{Content}}", content, StringComparison.OrdinalIgnoreCase);
    }

    private string ReadTemplate(string fileName)
    {
        var path = Path.Combine(_templatesRoot, fileName);
        if (!File.Exists(path))
            throw new FileNotFoundException($"Email template not found: {fileName}", path);
        return File.ReadAllText(path);
    }
}
