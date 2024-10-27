namespace AuthServer.Codes.Abstractions;
internal interface IAuthorizationCodeEncoder
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="authorizationCode"></param>
    /// <returns></returns>
    string EncodeAuthorizationCode(EncodedAuthorizationCode authorizationCode);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="authorizationCode"></param>
    /// <returns></returns>
    EncodedAuthorizationCode? DecodeAuthorizationCode(string? authorizationCode);
}