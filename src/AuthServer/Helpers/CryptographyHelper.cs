using System.Security.Cryptography;
using System.Text;

namespace AuthServer.Helpers;
internal static class CryptographyHelper
{
    private const string Characters = "0123456789!$()[]{}%abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";

    public static string GetRandomString(int length)
    {
        return RandomNumberGenerator.GetString(Characters, length);
    }

    public static string Sha256(this string data)
    {
        var bytes = Encoding.Default.GetBytes(data);
        var hash = SHA256.HashData(bytes);
        var builder = new StringBuilder();
        foreach (var b in hash)
        {
            builder.Append(b.ToString("X2"));
        }

        return builder.ToString();
    }
}