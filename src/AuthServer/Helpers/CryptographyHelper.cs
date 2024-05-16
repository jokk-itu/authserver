using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using System.Text;

namespace AuthServer.Helpers;
internal static class CryptographyHelper
{
    public static string GetUrlEncodedRandomString(int length)
    {
        return Base64UrlEncoder.Encode(RandomNumberGenerator.GetBytes(length));
    }

    public static string GetRandomString(int length)
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(length));
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