namespace USBInfilteratorDetectorServiceAPI.Services.Abstractions;

/// <summary>
/// Generates high-entropy Worker API keys.
/// </summary>
public interface IApiKeyGenerator
{
    /// <summary>
    /// Creates a new API key and its non-secret prefix.
    /// </summary>
    (string ApiKey, string Prefix) Generate();
}
