using System.Security.Cryptography;
using System.Text;

namespace CodeChallengeConsole;
public static class StringExtensions
{
  public static string Sha256UTF8(this string data)
  {
    using var sha = SHA256.Create();
    var bytes = Encoding.Default.GetBytes(data);
    var hash = sha.ComputeHash(bytes);
    var builder = new StringBuilder();
    foreach (var b in hash)
      builder.Append(b.ToString("X2"));
    return builder.ToString();
  }

  public static string Sha256ASCII(this string data)
  {
    using var sha = SHA256.Create();
    var bytes = Encoding.ASCII.GetBytes(data);
    var hash = sha.ComputeHash(bytes);
    var builder = new StringBuilder();
    foreach (var b in hash)
      builder.Append(b.ToString("X2"));
    return builder.ToString();
  }

  public static string Base64EncodeUTF8(this string plainText)
  {
    var plainTextBytes = Encoding.Default.GetBytes(plainText);
    return Convert.ToBase64String(plainTextBytes);
  }

  public static string Base64EncodeASCII(this string plainText)
  {
    var plainTextBytes = Encoding.ASCII.GetBytes(plainText);
    return Convert.ToBase64String(plainTextBytes);
  }
}
