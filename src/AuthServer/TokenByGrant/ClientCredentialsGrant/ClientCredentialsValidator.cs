using AuthServer.Cache.Abstractions;
using AuthServer.Constants;
using AuthServer.Core.Abstractions;
using AuthServer.Core.RequestProcessing;
using AuthServer.Extensions;
using AuthServer.Repositories.Abstractions;
using AuthServer.RequestAccessors.Token;

namespace AuthServer.TokenByGrant.ClientCredentialsGrant;

internal class ClientCredentialsValidator : IRequestValidator<TokenRequest, ClientCredentialsValidatedRequest>
{
    private readonly IClientAuthenticationService _clientAuthenticationService;
    private readonly ICachedClientStore _cachedClientStore;
    private readonly IClientRepository _clientRepository;

    public ClientCredentialsValidator(
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
        if (!TokenEndpointAuthMethodConstants.SecureAuthMethods.Contains(clientAuthentication.Method.GetDescription()))
        {
            return TokenError.InvalidClient;
        }

        var clientAuthenticationResult = await _clientAuthenticationService.AuthenticateClient(clientAuthentication, cancellationToken);

        if (!clientAuthenticationResult.IsAuthenticated)
        {
            return TokenError.InvalidClient;
        }

        var clientId = clientAuthenticationResult.ClientId!;
        var cachedClient = await _cachedClientStore.Get(clientId, cancellationToken);

        var isClientAuthorizedForClientCredentials = cachedClient.GrantTypes.Contains(request.GrantType);
        if (!isClientAuthorizedForClientCredentials)
        {
            return TokenError.UnauthorizedForGrantType;
        }

        if (request.Scope.All(x => cachedClient.Scopes.Any(y => y == x)))
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