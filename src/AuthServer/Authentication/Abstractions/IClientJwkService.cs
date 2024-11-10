using AuthServer.Authentication.Exceptions;
using Microsoft.IdentityModel.Tokens;

namespace AuthServer.Authentication.Abstractions;
public interface IClientJwkService
{
    /// <summary>
    /// Gets the clients Jwks and returns a single key usable for encryption.
    /// Throws <exception cref="ClientJwkRefreshException"></exception> if refreshing jwks fails.
    /// </summary>
    /// <param name="clientId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>key usable for encryption.</returns>
    Task<JsonWebKey?> GetEncryptionKey(string clientId, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the clients Jwks and returns all signing keys.
    /// </summary>
    /// <param name="clientId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<IEnumerable<JsonWebKey>> GetSigningKeys(string clientId, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the clients Jwks and returns all keys based on use.
    /// </summary>
    /// <param name="clientId"></param>
    /// <param name="use"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<IEnumerable<JsonWebKey>> GetKeys(string clientId, string use, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the jwks from the given jwksUri.
    /// This is only to be used in the Register flow, when validating JwksUri.
    /// </summary>
    /// <param name="jwksUri"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<string?> GetJwks(string jwksUri, CancellationToken cancellationToken);
}