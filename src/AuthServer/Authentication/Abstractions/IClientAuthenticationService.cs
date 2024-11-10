using AuthServer.Authentication.Models;

namespace AuthServer.Authentication.Abstractions;
internal interface IClientAuthenticationService
{
    /// <summary>
    /// Authenticates the client, based off its implementation.
    /// The returned result contains a ClientId
    /// and a boolean to determine if the client was authenticated.
    /// </summary>
    /// <param name="clientAuthentication"></param>
    /// <returns><see cref="ClientAuthenticationResult"/></returns>
    Task<ClientAuthenticationResult> AuthenticateClient(ClientAuthentication clientAuthentication, CancellationToken cancellationToken);
}