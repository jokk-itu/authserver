using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;

namespace Specs.Helpers;
public static class RandomGeneratorHelper
{
  public static string GeneratorRandomString(int length)
  {
    return Base64UrlEncoder.Encode(RandomNumberGenerator.GetBytes(length));
  }
}
