using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace AuthServer.Tests.IntegrationTest;

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