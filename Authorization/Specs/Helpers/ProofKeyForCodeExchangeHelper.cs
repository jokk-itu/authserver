using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using System.Text;

namespace Specs.Helpers;
public static class ProofKeyForCodeExchangeHelper
{
  public static (string, string) GetCodes()
  {
    var codeVerifier = "wilunhbgiwubnguiwebg";
    using var sha256 = SHA256.Create();
    var hashed = sha256.ComputeHash(Encoding.Default.GetBytes(codeVerifier));
    var codeChallenge = Base64UrlEncoder.Encode(hashed);
    return (codeVerifier, codeChallenge);
  }
}
