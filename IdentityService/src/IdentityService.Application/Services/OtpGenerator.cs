using System.Security.Cryptography;

namespace IdentityService.Application.Services;

/// <summary>
/// Stateless helper for generating and hashing OTP codes.
/// All methods are static so they can be called without DI.
/// </summary>
public static class OtpGenerator
{
    private const int OtpLength = 6;

    /// <summary>
    /// Generates a cryptographically random 6-digit numeric OTP string, e.g. "048271".
    /// Uses <see cref="RandomNumberGenerator"/> to avoid predictable sequences.
    /// </summary>
    public static string Generate()
    {
        // Generate a random number in [0, 1_000_000) and zero-pad to 6 digits.
        var randomBytes = new byte[4];
        RandomNumberGenerator.Fill(randomBytes);
        var randomValue = Math.Abs(BitConverter.ToInt32(randomBytes, 0)) % 1_000_000;
        return randomValue.ToString($"D{OtpLength}");
    }

    /// <summary>
    /// Returns the SHA-256 hex-encoded hash of the given plain-text OTP.
    /// The hash is stored in the database; the plain-text is only ever emailed to the user.
    /// </summary>
    public static string Hash(string otp)
    {
        var bytes = System.Text.Encoding.UTF8.GetBytes(otp);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash); // uppercase hex, 64 chars
    }

    /// <summary>
    /// Constant-time comparison to verify a plain-text OTP against its stored hash.
    /// </summary>
    public static bool Verify(string plainOtp, string storedHash)
    {
        var computedHash = Hash(plainOtp);
        // CryptographicOperations.FixedTimeEquals prevents timing attacks.
        var a = System.Text.Encoding.UTF8.GetBytes(computedHash);
        var b = System.Text.Encoding.UTF8.GetBytes(storedHash);
        return CryptographicOperations.FixedTimeEquals(a, b);
    }
}
