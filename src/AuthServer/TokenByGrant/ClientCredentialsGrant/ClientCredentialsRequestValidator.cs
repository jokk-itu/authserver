using AuthServer.Authentication;
using AuthServer.Authentication.Abstractions;
using AuthServer.Cache.Abstractions;
using AuthServer.Constants;
using AuthServer.Core.Abstractions;
using AuthServer.Core.Request;
using AuthServer.Extensions;
using AuthServer.Repositories.Abstractions;
using AuthServer.RequestAccessors.Token;

namespace AuthServer.TokenByGrant.ClientCredentialsGrant;

internal class ClientCredentialsRequestValidator : IRequestValidator<TokenRequest, ClientCredentialsValidatedRequest>
{
    private readonly IClientAuthenticationService _clientAuthenticationService;
    private readonly ICachedClientStore _cachedClientStore;
    private readonly IClientRepository _clientRepository;

    public ClientCredentialsRequestValidator(
        IClientAuthenticationService clientAuthenticationService,
        ICachedClientStore cachedClientStore,
        IClientRepository clientRepository)
    {
        _clientAuthenticationService = clientAuthenticationService;
        _cachedClientStore = cachedClientStore;
        _clientRepository = clientRepository;
    }

    public async Task<ProcessResult<ClientCredentialsValidatedRequest, ProcessError>> Validate(TokenRequest request, CancellationToken cancellationToken)
    {
        if (request.GrantType != GrantTypeConstants.ClientCredentials)
        {
            return TokenError.UnsupportedGrantType;
        }

        if (request.Scope.Count == 0)
        {
            return TokenError.InvalidScope;
        }

        if (request.Resource.Count == 0)
        {
            return TokenError.InvalidTarget;
        }

        var isClientAuthenticationMethodInvalid = request.ClientAuthentications.Count != 1;
        if (isClientAuthenticationMethodInvalid)
        {
            return TokenError.MultipleOrNoneClientMethod;
        }

        var clientAuthentication = request.ClientAuthentications.Single();
        var clientAuthenticationResult = await _clientAuthenticationService.AuthenticateClient(clientAuthentication, cancellationToken);
        if (!clientAuthenticationResult.IsAuthenticated)
        {
            return TokenError.InvalidClient;
        }

        var clientId = clientAuthenticationResult.ClientId!;
        var cachedClient = await _cachedClientStore.Get(clientId, cancellationToken);

        var isClientAuthorizedForClientCredentials = cachedClient.GrantTypes.Contains(GrantTypeConstants.ClientCredentials);
        if (!isClientAuthorizedForClientCredentials)
        {
            return TokenError.UnauthorizedForGrantType;
        }

        if (request.Scope.ExceptAny(cachedClient.Scopes))
        {
            return TokenError.UnauthorizedForScope;
        }

        var doesResourcesExist = await _clientRepository.DoesResourcesExist(request.Resource, request.Scope, cancellationToken);
        if (!doesResourcesExist)
        {
            return TokenError.InvalidTarget;
        }

        return new ClientCredentialsValidatedRequest
        {
            ClientId = clientId,
            Scope = request.Scope,
            Resource = request.Resource
        };
    }
}