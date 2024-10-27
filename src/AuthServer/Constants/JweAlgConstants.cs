namespace AuthServer.Constants;

/// <summary>
/// https://datatracker.ietf.org/doc/html/rfc7518#section-4.1
/// </summary>
public static class JweAlgConstants
{
    public const string RsaPKCS1 = "RSA1_5";
    public const string RsaOAEP = "RSA-OAEP";
    public const string RsaOAEP256 = "RSA-OAEP-256";
    public const string EcdhEsA128KW = "ECDH-ES+A128KW";
    public const string EcdhEsA192KW = "ECDH-ES+A192KW";
    public const string EcdhEsA256KW = "ECDH-ES+A256KW";

    public static readonly string[] AlgValues =
        [RsaPKCS1, RsaOAEP, RsaOAEP256, EcdhEsA128KW, EcdhEsA192KW, EcdhEsA256KW];
}