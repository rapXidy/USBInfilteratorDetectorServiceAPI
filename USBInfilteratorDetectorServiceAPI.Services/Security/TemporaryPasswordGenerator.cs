using System.Security.Cryptography;

namespace USBInfilteratorDetectorServiceAPI.Services.Security;

/// <summary>
/// Generates cryptographically random temporary passwords for auditor provisioning and resets.
/// </summary>
public static class TemporaryPasswordGenerator
{
    private const string Alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz23456789!@$%*?";
    private const int DefaultLength = 16;

    /// <summary>
    /// Creates a temporary password of the default length.
    /// </summary>
    public static string Generate(int length = DefaultLength)
    {
        if (length < 8)
            throw new ArgumentOutOfRangeException(nameof(length), "Password length must be at least 8.");

        var chars = new char[length];
        var bytes = RandomNumberGenerator.GetBytes(length);
        for (var i = 0; i < length; i++)
            chars[i] = Alphabet[bytes[i] % Alphabet.Length];
        return new string(chars);
    }
}
