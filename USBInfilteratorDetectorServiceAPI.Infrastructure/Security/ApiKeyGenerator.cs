using System.Security.Cryptography;
using USBInfilteratorDetectorServiceAPI.Services.Abstractions;

namespace USBInfilteratorDetectorServiceAPI.Infrastructure.Security;

/// <summary>
/// Generates high-entropy Worker API keys.
/// </summary>
public class ApiKeyGenerator : IApiKeyGenerator
{
    /// <inheritdoc />
    public (string ApiKey, string Prefix) Generate()
    {
        var key = $"uid_{Convert.ToHexString(RandomNumberGenerator.GetBytes(24)).ToLowerInvariant()}";
        var prefix = key.Length >= 12 ? key[..12] : key;
        return (key, prefix);
    }
}
