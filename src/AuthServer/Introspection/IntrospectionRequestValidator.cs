using AuthServer.Authentication;
using AuthServer.Authentication.Abstractions;
using AuthServer.Cache.Abstractions;
using AuthServer.Constants;
using AuthServer.Core.Abstractions;
using AuthServer.Core.Request;
using AuthServer.Extensions;
using AuthServer.RequestAccessors.Introspection;

namespace AuthServer.Introspection;
internal class IntrospectionRequestValidator : IRequestValidator<IntrospectionRequest, IntrospectionValidatedRequest>
{
    private readonly IClientAuthenticationService _clientAuthenticationService;
    private readonly ICachedClientStore _cachedClientStore;

    public IntrospectionRequestValidator(
        IClientAuthenticationService clientAuthenticationService,
        ICachedClientStore cachedClientStore)
    {
        _clientAuthenticationService = clientAuthenticationService;
        _cachedClientStore = cachedClientStore;
    }

    public async Task<ProcessResult<IntrospectionValidatedRequest, ProcessError>> Validate(IntrospectionRequest request, CancellationToken cancellationToken)
    {
        var isTokenTypeHintInvalid = !string.IsNullOrWhiteSpace(request.TokenTypeHint)
                                     && !TokenTypeConstants.TokenTypes.Contains(request.TokenTypeHint);

        if (isTokenTypeHintInvalid)
        {
            return IntrospectionError.UnsupportedTokenType;
        }

        /*
         * the token parameter is required per rf 7662,
         * and if the value itself is allowed to be invalid
         */
        var isTokenInvalid = string.IsNullOrWhiteSpace(request.Token);
        if (isTokenInvalid)
        {
            return IntrospectionError.EmptyToken;
        }

        var isClientAuthenticationMethodInvalid = request.ClientAuthentications.Count != 1;
        if (isClientAuthenticationMethodInvalid)
        {
            return IntrospectionError.MultipleOrNoneClientMethod;
        }

        var clientAuthentication = request.ClientAuthentications.Single();
        if (!IntrospectionEndpointAuthMethodConstants.AuthMethods.Contains(clientAuthentication.Method.GetDescription()))
        {
            return IntrospectionError.InvalidClient;
        }

        var clientAuthenticationResult = await _clientAuthenticationService.AuthenticateClient(clientAuthentication, cancellationToken);
        if (!clientAuthenticationResult.IsAuthenticated || string.IsNullOrWhiteSpace(clientAuthenticationResult.ClientId))
        {
            return IntrospectionError.InvalidClient;
        }

        var cachedClient = await _cachedClientStore.Get(clientAuthenticationResult.ClientId, cancellationToken);

        return new IntrospectionValidatedRequest
        {
            ClientId = clientAuthenticationResult.ClientId,
            Token = request.Token!,
            Scope = cachedClient.Scopes
        };
    }
}