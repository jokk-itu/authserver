using AuthServer.Cache;
using AuthServer.Constants;
using AuthServer.Core.Abstractions;
using AuthServer.Core.Models;
using AuthServer.Enums;
using AuthServer.TokenDecoders;
using Microsoft.Extensions.Logging;

namespace AuthServer.Core;
internal class ClientAuthenticationService : IClientAuthenticationService
{
    private readonly ILogger<ClientAuthentication> _logger;
    private readonly ICachedClientStore _cachedClientStore;
    private readonly ITokenDecoder<ClientIssuedTokenDecodeArguments> _clientIssuedTokenDecoder;

    public ClientAuthenticationService(
        ILogger<ClientAuthentication> logger,
        ICachedClientStore cachedClientStore,
        ITokenDecoder<ClientIssuedTokenDecodeArguments> clientIssuedTokenDecoder)
    {
        _logger = logger;
        _cachedClientStore = cachedClientStore;
        _clientIssuedTokenDecoder = clientIssuedTokenDecoder;
    }

    public async Task<ClientAuthenticationResult> AuthenticateClient(ClientAuthentication clientAuthentication, CancellationToken cancellationToken)
    {
        return clientAuthentication switch
        {
            ClientIdAuthentication clientIdAuthentication => await AuthenticateClientId(clientIdAuthentication, cancellationToken),
            ClientSecretAuthentication clientSecretAuthentication => await AuthenticateClientSecret(
                clientSecretAuthentication, cancellationToken),
            ClientAssertionAuthentication clientAssertionAuthentication => await AuthenticateClientAssertion(
                clientAssertionAuthentication, cancellationToken),
            _ => throw new NotSupportedException("authentication method is unsupported")
        };
    }

    private async Task<ClientAuthenticationResult> AuthenticateClientId(ClientIdAuthentication clientIdAuthentication, CancellationToken cancellationToken)
    {
        var client = await _cachedClientStore.TryGet(clientIdAuthentication.ClientId, cancellationToken);

        if (client is null)
        {
            _logger.LogTrace("ClientId {ClientId} does not exist", clientIdAuthentication.ClientId);
            return new ClientAuthenticationResult(null, false);
        }

        if (client.TokenEndpointAuthMethod != TokenEndpointAuthMethod.None)
        {
            return new ClientAuthenticationResult(null, false);
        }

        return new ClientAuthenticationResult(clientIdAuthentication.ClientId, true);
    }

    private async Task<ClientAuthenticationResult> AuthenticateClientSecret(ClientSecretAuthentication clientSecretAuthentication, CancellationToken cancellationToken)
    {
        var client = await _cachedClientStore.TryGet(clientSecretAuthentication.ClientId, cancellationToken);

        if (client is null)
        {
            _logger.LogTrace("ClientId {ClientId} does not exist", clientSecretAuthentication.ClientId);
            return new ClientAuthenticationResult(null, false);
        }

        var isClientAuthorizedForAuthenticationMethod = new[] { TokenEndpointAuthMethod.ClientSecretBasic, TokenEndpointAuthMethod.ClientSecretPost }
            .Contains(client.TokenEndpointAuthMethod);

        if (!isClientAuthorizedForAuthenticationMethod)
        {
            return new ClientAuthenticationResult(null, false);
        }

        if (client.SecretExpiresAt is not null
            && client.SecretExpiresAt < DateTime.UtcNow)
        {
            _logger.LogTrace("ClientSecret has expired at {Expiration}", client.SecretExpiresAt);
            return new ClientAuthenticationResult(null, false);
        }

        if (string.IsNullOrWhiteSpace(clientSecretAuthentication.ClientSecret)
            || !BCrypt.CheckPassword(clientSecretAuthentication.ClientSecret, client.SecretHash))
        {
            _logger.LogTrace("ClientSecret is invalid");
            return new ClientAuthenticationResult(null, false);
        }

        return new ClientAuthenticationResult(client.Id, true);
    }

    private async Task<ClientAuthenticationResult> AuthenticateClientAssertion(ClientAssertionAuthentication clientAssertionAuthentication, CancellationToken cancellationToken)
    {
        var clientAssertionIsPrivateKey = clientAssertionAuthentication.ClientAssertionType ==
                                          ClientAssertionTypeConstants.PrivateKeyJwt;

        if (!clientAssertionIsPrivateKey)
        {
            return new ClientAuthenticationResult(null, false);
        }

        var clientId = clientAssertionAuthentication.ClientId;
        if (string.IsNullOrWhiteSpace(clientId))
        {
            var unvalidatedToken = await _clientIssuedTokenDecoder.Read(clientAssertionAuthentication.ClientAssertion);
            clientId = unvalidatedToken.Issuer;
        }

        var client = await _cachedClientStore.TryGet(clientId, cancellationToken);

        if (client is null)
        {
            _logger.LogTrace("ClientId {ClientId} does not exist", clientId);
            return new ClientAuthenticationResult(null, false);
        }

        if (client.TokenEndpointAuthMethod != TokenEndpointAuthMethod.PrivateKeyJwt)
        {
            return new ClientAuthenticationResult(null, false);
        }

        try
        {
            await _clientIssuedTokenDecoder.Validate(clientAssertionAuthentication.ClientAssertion, new ClientIssuedTokenDecodeArguments
            {
                TokenTypes = [TokenTypeHeaderConstants.PrivateKeyToken],
                ClientId = clientId,
                Audience = clientAssertionAuthentication.Audience,
                ValidateLifetime = true
            }, cancellationToken);

            return new ClientAuthenticationResult(clientId, true);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Token validation failed");
            return new ClientAuthenticationResult(null, false);
        }
    }
}