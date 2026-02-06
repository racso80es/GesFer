using System.Security.Cryptography;
using System.Text;

namespace GesFer.Shared.Back.Domain.Services;

public class SensitiveDataSanitizer : ISensitiveDataSanitizer
{
    private static readonly char[] PasswordChars =
        "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!@#$%^&*()_-+=<>?".ToCharArray();

    public string GenerateRandomPassword(int length = 12)
    {
        if (length < 1) throw new ArgumentOutOfRangeException(nameof(length));

        var sb = new StringBuilder(length);
        var bytes = new byte[length];

        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(bytes);
        }

        foreach (var b in bytes)
        {
            sb.Append(PasswordChars[b % PasswordChars.Length]);
        }

        return sb.ToString();
    }

    public string GenerateRandomEmail(string? prefix = null, string domain = "gesfer.local")
    {
        var p = string.IsNullOrWhiteSpace(prefix)
            ? $"user_{GenerateRandomString(6)}"
            : prefix;

        return $"{p}@{domain}";
    }

    public string Sanitize(string input)
    {
        // Simple sanitization or placeholder logic if needed
        return input;
    }

    private string GenerateRandomString(int length)
    {
        const string chars = "abcdefghijklmnopqrstuvwxyz0123456789";
        var sb = new StringBuilder(length);
        var bytes = new byte[length];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(bytes);
        }
        foreach (var b in bytes)
        {
            sb.Append(chars[b % chars.Length]);
        }
        return sb.ToString();
    }
}
