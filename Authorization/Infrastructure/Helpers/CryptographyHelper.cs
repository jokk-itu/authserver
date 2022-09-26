using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;

namespace Infrastructure.Helpers;
public static class CryptographyHelper
{
  public static string GetUrlEncodedRandomString(int length)
  {
    return Base64UrlEncoder.Encode(RandomNumberGenerator.GetBytes(length));
  }

  public static string GetRandomString(int length)
  {
    return Convert.ToBase64String(RandomNumberGenerator.GetBytes(length));
  }
}