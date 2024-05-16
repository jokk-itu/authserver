using AuthServer.Constants;
using AuthServer.Core;
using AuthServer.Core.Abstractions;
using AuthServer.Core.RequestProcessing;
using AuthServer.Entities;
using AuthServer.Extensions;
using AuthServer.RequestAccessors.Introspection;
using Microsoft.EntityFrameworkCore;

namespace AuthServer.Introspection;
internal class IntrospectionRequestValidator : IRequestValidator<IntrospectionRequest, IntrospectionValidatedRequest>
{
    private readonly IdentityContext _identityContext;
    private readonly IClientAuthenticationService _clientAuthenticationService;

    public IntrospectionRequestValidator(
        IdentityContext identityContext,
        IClientAuthenticationService clientAuthenticationService)
    {
        _identityContext = identityContext;
        _clientAuthenticationService = clientAuthenticationService;
    }

    public async Task<ProcessResult<IntrospectionValidatedRequest, ProcessError>> Validate(IntrospectionRequest request, CancellationToken cancellationToken)
    {
        var isTokenTypeHintInvalid = !string.IsNullOrWhiteSpace(request.TokenTypeHint)
                                     && !TokenTypeConstants.TokenTypes.Contains(request.TokenTypeHint);

        if (isTokenTypeHintInvalid)
        {
            return IntrospectionError.UnsupportedTokenType;
        }

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
        if (IntrospectionEndpointAuthMethodConstants.AuthMethods.Contains(clientAuthentication.Method.GetDescription()))
        {
            return IntrospectionError.InvalidClient;
        }

        var clientAuthenticationResult = await _clientAuthenticationService.AuthenticateClient(clientAuthentication, cancellationToken);
        if (!clientAuthenticationResult.IsAuthenticated || string.IsNullOrWhiteSpace(clientAuthenticationResult.ClientId))
        {
            return IntrospectionError.InvalidClient;
        }

        var error = await AuthorizeClient(request.Token, cancellationToken);
        if (error is not null)
        {
            return error;
        }

        return new IntrospectionValidatedRequest
        {
            ClientId = clientAuthenticationResult.ClientId,
            Token = request.Token
        };
    }

    private async Task<ProcessError?> AuthorizeClient(string referenceToken, CancellationToken cancellationToken)
    {
        var query = await _identityContext
            .Set<Token>()
            .Where(x => x.Reference == referenceToken)
            .Select(x => new
            {
                Token = x,
                ClientIdFromGrant = (x as GrantToken)!.AuthorizationGrant.Client.Id,
                ClientIdFromClientToken = (x as ClientAccessToken)!.Client.Id
            })
            .SingleOrDefaultAsync(cancellationToken: cancellationToken);

        var clientIdFromToken = query?.ClientIdFromClientToken ?? query?.ClientIdFromGrant;

        if (string.IsNullOrWhiteSpace(clientIdFromToken))
        {
            return null;
        }

        var client = await _identityContext
            .Set<Client>()
            .Include(x => x.Scopes)
            .SingleAsync(cancellationToken: cancellationToken);

        var scope = query!.Token.Scope?.Split(' ') ?? [];
        var isAuthorizedForScope = client.Scopes.Select(s => s.Name).Intersect(scope).Any();
        return !isAuthorizedForScope ? IntrospectionError.ClientIsUnauthorizedForScope : null;
    }
}