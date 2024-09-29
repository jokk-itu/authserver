using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using AuthServer.Constants;
using Microsoft.IdentityModel.Tokens;

namespace AuthServer.Helpers;

internal static class ProofKeyForCodeExchangeHelper
{
    public static bool IsCodeChallengeMethodValid(string? codeChallengeMethod)
        => CodeChallengeMethodConstants.CodeChallengeMethods.Contains(codeChallengeMethod);

    public static bool IsCodeChallengeValid(string? codeChallenge)
    {
        if (string.IsNullOrWhiteSpace(codeChallenge))
        {
            return false;
        }

        return IsCodeValid(codeChallenge);
    }

    public static bool IsCodeVerifierValid(string? codeVerifier, string? codeChallenge)
    {
        if (string.IsNullOrWhiteSpace(codeVerifier)
            || string.IsNullOrWhiteSpace(codeChallenge))
        {
            return false;
        }

        var isCodeVerifierFormatValid = IsCodeValid(codeVerifier);
        if (!isCodeVerifierFormatValid)
        {
            return false;
        }

        var bytes = Encoding.UTF8.GetBytes(codeVerifier);
        var hashed = SHA256.HashData(bytes);
        var encoded = Base64UrlEncoder.Encode(hashed);
        return encoded == codeChallenge;
    }

    private static bool IsCodeValid(string code) => Regex.IsMatch(
        code,
        "^[0-9a-zA-Z-_~.]{43,128}$",
        RegexOptions.Compiled,
        TimeSpan.FromMilliseconds(100));
}