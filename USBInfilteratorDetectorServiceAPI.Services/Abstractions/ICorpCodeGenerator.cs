namespace USBInfilteratorDetectorServiceAPI.Services.Abstractions;

/// <summary>
/// Generates unique organization corporate codes.
/// </summary>
public interface ICorpCodeGenerator
{
    /// <summary>
    /// Builds a candidate CorpCode from an organization name.
    /// </summary>
    string Generate(string organizationName);
}
