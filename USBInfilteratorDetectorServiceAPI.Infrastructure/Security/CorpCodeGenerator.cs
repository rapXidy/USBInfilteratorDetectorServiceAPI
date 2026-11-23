using System.Security.Cryptography;
using System.Text.RegularExpressions;
using USBInfilteratorDetectorServiceAPI.Services.Abstractions;

namespace USBInfilteratorDetectorServiceAPI.Infrastructure.Security;

/// <summary>
/// Generates organization CorpCodes from a sanitized name prefix plus random segment.
/// </summary>
public class CorpCodeGenerator : ICorpCodeGenerator
{
    /// <inheritdoc />
    public string Generate(string organizationName)
    {
        var prefix = Regex.Replace(organizationName.ToUpperInvariant(), "[^A-Z0-9]", string.Empty);
        if (prefix.Length > 8)
            prefix = prefix[..8];
        if (string.IsNullOrWhiteSpace(prefix))
            prefix = "ORG";

        var suffix = Convert.ToHexString(RandomNumberGenerator.GetBytes(3));
        return $"{prefix}-{suffix}";
    }
}
