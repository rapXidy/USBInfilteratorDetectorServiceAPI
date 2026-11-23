namespace USBInfilteratorDetectorServiceAPI.Services.Abstractions;

/// <summary>
/// Hashes and verifies secrets using Argon2id.
/// </summary>
public interface IPasswordHasher
{
    /// <summary>
    /// Creates an Argon2id hash for the provided plaintext.
    /// </summary>
    string Hash(string plaintext);

    /// <summary>
    /// Verifies plaintext against a stored Argon2id hash.
    /// </summary>
    bool Verify(string plaintext, string hash);
}
