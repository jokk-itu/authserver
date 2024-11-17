using Microsoft.IdentityModel.JsonWebTokens;

namespace AuthServer.TokenDecoders.Abstractions;
internal interface ITokenDecoder<in TArguments>
    where TArguments : class
{
    /// <summary>
    /// Validates and returns a <see cref="JsonWebToken"/>
    /// Always validates signature
    /// Always validates typ JOSE header
    /// Optionally validates exp claim
    /// Optionally validates aud claim
    ///
    /// If validation fails, then a <see cref="Exception"/> is thrown.
    /// </summary>
    /// <param name="token"></param>
    /// <param name="arguments"></param>
    /// <param name="cancellationToken"></param>
    /// <returns><see cref="JsonWebToken"/></returns>
    Task<JsonWebToken?> Validate(string token, TArguments arguments, CancellationToken cancellationToken);

    /// <summary>
    /// Reads a token, if encrypted then decrypts and then reads payload without validation.
    /// It does not throw.
    /// </summary>
    /// <param name="token"></param>
    /// <returns><see cref="JsonWebToken"/></returns>
    Task<JsonWebToken> Read(string token);
}