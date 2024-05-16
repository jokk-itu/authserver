namespace AuthServer.Constants;

/// <summary>
/// https://datatracker.ietf.org/doc/html/rfc7518#section-5.1
/// </summary>
public static class JweEncConstants
{
    public const string Aes128CbcHmacSha256 = "A128CBC-HS256";
    public const string Aes192CbcHmacSha384 = "A192CBC-HS384";
    public const string Aes256CbcHmacSha512 = "A256CBC-HS512";
}