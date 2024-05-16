using System.Security.Cryptography;
using System.Text;

namespace Infrastructure.Extensions;

public static class CryptographyExtensions
{
  public static string Sha256(this string data)
  {
    using var sha = SHA256.Create();
    var bytes = Encoding.Default.GetBytes(data);
    var hash = sha.ComputeHash(bytes);
    var builder = new StringBuilder();
    foreach (var b in hash)
      builder.Append(b.ToString("X2"));
    return builder.ToString();
  }

  public static string Base64Encode(this string plainText)
  {
    var plainTextBytes = Encoding.Default.GetBytes(plainText);
    return Convert.ToBase64String(plainTextBytes);
  }

  public static string Base64Decode(this string base64EncodedData)
  {
    var base64EncodedBytes = Convert.FromBase64String(base64EncodedData);
    return Encoding.Default.GetString(base64EncodedBytes);
  }
}