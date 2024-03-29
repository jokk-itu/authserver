﻿using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using System.Text;

namespace Specs.Helpers;
public static class ProofKeyForCodeExchangeHelper
{
  public static Pkce GetPkce()
  {
    var codeVerifier = Base64UrlEncoder.Encode(RandomNumberGenerator.GetBytes(64));
    using var sha256 = SHA256.Create();
    var hashed = sha256.ComputeHash(Encoding.Default.GetBytes(codeVerifier));
    var codeChallenge = Base64UrlEncoder.Encode(hashed);
    return new Pkce(codeChallenge, codeVerifier);
  }
}

public class Pkce
{
  public Pkce(string codeChallenge, string codeVerifier)
  {
    CodeChallenge = codeChallenge;
    CodeVerifier = codeVerifier;
  }
  public string CodeChallenge { get; init; }
  public string CodeVerifier { get; init; }
}