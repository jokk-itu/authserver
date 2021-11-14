using System.Security.Cryptography;
using System.Text;

namespace OAuthService;

public static class CryptographyExtensions
{
    public static string Sha256(this string data)
    {
        using var sha = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(data);
        var hash = sha.ComputeHash(bytes);
        var builder = new StringBuilder();
        foreach (var b in hash)
            builder.Append(b.ToString("X2"));
        return builder.ToString();
    }
}