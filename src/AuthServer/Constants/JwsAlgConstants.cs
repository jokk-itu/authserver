namespace AuthServer.Constants;

/// <summary>
///  https://datatracker.ietf.org/doc/html/rfc7518#section-3
/// </summary>
public static class JwsAlgConstants
{
    public const string EcdsaSha256 = "ES256";
    public const string EcdsaSha384 = "ES384";
    public const string EcdsaSha512 = "ES512";

    public const string RsaSha256 = "RS256";
    public const string RsaSha384 = "RS384";
    public const string RsaSha512 = "RS512";

    public const string RsaSsaPssSha256 = "PS256";
    public const string RsaSsaPssSha384 = "PS384";
    public const string RsaSsaPssSha512 = "PS512";

    public static readonly string[] AlgValues =
    [
        EcdsaSha256, EcdsaSha384, EcdsaSha512, RsaSha256, RsaSha384, RsaSha512, RsaSsaPssSha256, RsaSsaPssSha384,
        RsaSsaPssSha512
    ];
}