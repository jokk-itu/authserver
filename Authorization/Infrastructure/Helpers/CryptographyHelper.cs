using System.Security.Cryptography;

namespace Infrastructure.Helpers;
public static class CryptographyHelper
{
  public static string RandomSecret(long size)
  {
    var data = new byte[size];
    RandomNumberGenerator.Create().GetBytes(data);
    return Convert.ToBase64String(data);
  }
}
