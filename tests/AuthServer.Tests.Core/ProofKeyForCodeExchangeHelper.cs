using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using System.Text;

namespace AuthServer.Tests.Core;

public static class ProofKeyForCodeExchangeHelper
{
    public static ProofKeyForCodeExchange GetProofKeyForCodeExchange()
    {
        var codeVerifier = Base64UrlEncoder.Encode(RandomNumberGenerator.GetBytes(64));
        var hashed = SHA256.HashData(Encoding.Default.GetBytes(codeVerifier));
        var codeChallenge = Base64UrlEncoder.Encode(hashed);
        return new ProofKeyForCodeExchange(codeChallenge, codeVerifier);
    }
}

public record ProofKeyForCodeExchange(string CodeChallenge, string CodeVerifier);