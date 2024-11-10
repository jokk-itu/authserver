using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace AuthServer.Helpers;

internal static class TokenHelper
{
    public static bool IsJws(string token)
    {
        return JwtTokenUtilities.RegexJws.IsMatch(token);
    }

    public static bool IsJwe(string token)
    {
        return JwtTokenUtilities.RegexJwe.IsMatch(token);
    }

    public static bool IsJsonWebToken(string token)
    {
        return GetDotLength(token) is 3 or 5;
    }

    private static int GetDotLength(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new ArgumentException("must not be null or whitespace", token);
        }

        return token.Split('.').Length;
    }

    public static SecurityKey ConvertToSecurityKey(JsonWebKey jsonWebKey, bool exportPrivateKey)
    {
        if (jsonWebKey.Kty == JsonWebAlgorithmsKeyTypes.RSA
            && jsonWebKey.X5c.Count != 0)
        {
            return ConvertToX509(jsonWebKey);
        }

        if (jsonWebKey.Kty == JsonWebAlgorithmsKeyTypes.RSA
            && jsonWebKey.X5c.Count == 0)
        {
            return ConvertToRsa(jsonWebKey);
        }

        if (jsonWebKey.Kty == JsonWebAlgorithmsKeyTypes.EllipticCurve)
        {
            return ConvertToEcdsa(jsonWebKey, exportPrivateKey);
        }

        throw new ArgumentException($"Kty: {jsonWebKey.Kty} is unsupported", nameof(jsonWebKey));
    }

    public static ECDsaSecurityKey ConvertToEcdsa(JsonWebKey jsonWebKey, bool exportPrivateKey)
    {
        return new ECDsaSecurityKey(ECDsa.Create(new ECParameters
        {
            Curve = GetNamedECCurve(jsonWebKey.Crv),
            Q = new ECPoint
            {
                X = Base64UrlEncoder.DecodeBytes(jsonWebKey.X),
                Y = Base64UrlEncoder.DecodeBytes(jsonWebKey.Y)
            },
            D = exportPrivateKey ? Base64UrlEncoder.DecodeBytes(jsonWebKey.D) : null
        }));
    }

    public static RsaSecurityKey ConvertToRsa(JsonWebKey jsonWebKey)
    {
        return new RsaSecurityKey(RSA.Create(new RSAParameters
        {
            Modulus = Base64UrlEncoder.DecodeBytes(jsonWebKey.N),
            Exponent = Base64UrlEncoder.DecodeBytes(jsonWebKey.E)
        }));
    }

    public static X509SecurityKey ConvertToX509(JsonWebKey jsonWebKey)
    {
        return new X509SecurityKey(new X509Certificate2(Convert.FromBase64String(jsonWebKey.X5c[0])));
    }

    private static ECCurve GetNamedECCurve(string curveId)
    {
        return curveId switch
        {
            JsonWebKeyECTypes.P256 => ECCurve.NamedCurves.nistP256,
            JsonWebKeyECTypes.P384 => ECCurve.NamedCurves.nistP384,
            JsonWebKeyECTypes.P521 => ECCurve.NamedCurves.nistP521,
            _ => throw new ArgumentException("unknown curve", nameof(curveId))
        };
    }
}