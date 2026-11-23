using System.Security.Cryptography;
using System.Text;
using Konscious.Security.Cryptography;
using USBInfilteratorDetectorServiceAPI.Services.Abstractions;

namespace USBInfilteratorDetectorServiceAPI.Infrastructure.Security;

/// <summary>
/// Argon2id password and API-key hasher.
/// </summary>
public class Argon2PasswordHasher : IPasswordHasher
{
    private const int SaltSize = 16;
    private const int HashSize = 32;
    private const int Iterations = 4;
    private const int MemorySizeKb = 65536;
    private const int DegreeOfParallelism = 2;

    /// <inheritdoc />
    public string Hash(string plaintext)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = HashCore(plaintext, salt);
        return $"argon2id${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
    }

    /// <inheritdoc />
    public bool Verify(string plaintext, string hash)
    {
        var parts = hash.Split('$', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 3 || !string.Equals(parts[0], "argon2id", StringComparison.Ordinal))
            return false;

        var salt = Convert.FromBase64String(parts[1]);
        var expected = Convert.FromBase64String(parts[2]);
        var actual = HashCore(plaintext, salt);
        return CryptographicOperations.FixedTimeEquals(actual, expected);
    }

    private static byte[] HashCore(string plaintext, byte[] salt)
    {
        using var argon2 = new Argon2id(Encoding.UTF8.GetBytes(plaintext))
        {
            Salt = salt,
            DegreeOfParallelism = DegreeOfParallelism,
            MemorySize = MemorySizeKb,
            Iterations = Iterations
        };
        return argon2.GetBytes(HashSize);
    }
}
