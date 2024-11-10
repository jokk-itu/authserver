using AuthServer.Authentication;
using AuthServer.Authentication.Abstractions;
using AuthServer.Constants;
using AuthServer.Core;
using AuthServer.Core.Abstractions;
using AuthServer.Core.Request;
using AuthServer.Entities;
using AuthServer.Extensions;
using AuthServer.Helpers;
using AuthServer.RequestAccessors.Revocation;
using AuthServer.TokenDecoders;
using AuthServer.TokenDecoders.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace AuthServer.Revocation;
internal class RevocationRequestValidator : IRequestValidator<RevocationRequest, RevocationValidatedRequest>
{
    private readonly AuthorizationDbContext _identityContext;
    private readonly ITokenDecoder<ServerIssuedTokenDecodeArguments> _serverIssuedTokenDecoder;
    private readonly IClientAuthenticationService _clientAuthenticationService;

    public RevocationRequestValidator(
        AuthorizationDbContext identityContext,
        ITokenDecoder<ServerIssuedTokenDecodeArguments> serverIssuedTokenDecoder,
        IClientAuthenticationService clientAuthenticationService)
    {
        _identityContext = identityContext;
        _serverIssuedTokenDecoder = serverIssuedTokenDecoder;
        _clientAuthenticationService = clientAuthenticationService;
    }

    public async Task<ProcessResult<RevocationValidatedRequest, ProcessError>> Validate(RevocationRequest request, CancellationToken cancellationToken)
    {
        var isTokenTypeHintInvalid = !string.IsNullOrWhiteSpace(request.TokenTypeHint)
                                     && !TokenTypeConstants.TokenTypes.Contains(request.TokenTypeHint);

        if (isTokenTypeHintInvalid)
        {
            return RevocationError.UnsupportedTokenType;
        }

        /*
         * the token parameter is required per rf 7009,
         * and if the value itself is allowed to be invalid
         */
        var isTokenInvalid = string.IsNullOrWhiteSpace(request.Token);
        if (isTokenInvalid)
        {
            return RevocationError.EmptyToken;
        }

        var token = request.Token!;

        var isClientAuthenticationMethodInvalid = request.ClientAuthentications.Count != 1;
        if (isClientAuthenticationMethodInvalid)
        {
            return RevocationError.MultipleOrNoneClientMethod;
        }

        var clientAuthentication = request.ClientAuthentications.Single();
        if (!RevocationEndpointAuthMethodConstants.AuthMethods.Contains(clientAuthentication.Method.GetDescription()))
        {
            return RevocationError.InvalidClient;
        }

        var clientAuthenticationResult = await _clientAuthenticationService.AuthenticateClient(clientAuthentication, cancellationToken);
        if (!clientAuthenticationResult.IsAuthenticated || string.IsNullOrWhiteSpace(clientAuthenticationResult.ClientId))
        {
            return RevocationError.InvalidClient;
        }

        string? clientIdFromToken;
        if (TokenHelper.IsJsonWebToken(token))
        {
            clientIdFromToken = await GetClientIdFromStructuredToken(token, cancellationToken);
        }
        else
        {
            clientIdFromToken = await GetClientIdFromReferenceToken(token, cancellationToken);
        }

        if (!string.IsNullOrWhiteSpace(clientIdFromToken) && clientAuthenticationResult.ClientId != clientIdFromToken)
        {
            return RevocationError.ClientIdDoesNotMatchToken;
        }

        return new RevocationValidatedRequest
        {
            Token = token
        };
    }

    private async Task<string?> GetClientIdFromStructuredToken(string token, CancellationToken cancellationToken)
    {
        var validatedToken = await _serverIssuedTokenDecoder.Validate(token, new ServerIssuedTokenDecodeArguments
        {
            ValidateLifetime = false,
            Audiences = [],
            TokenTypes = [TokenTypeHeaderConstants.AccessToken, TokenTypeHeaderConstants.RefreshToken]
        }, cancellationToken);

        if (validatedToken is null)
        {
            return null;
        }

        validatedToken.TryGetClaim(ClaimNameConstants.ClientId, out var claim);
        return claim?.Value;
    }

    private async Task<string?> GetClientIdFromReferenceToken(string token, CancellationToken cancellationToken)
    {
        var clientToken = await _identityContext
            .Set<Token>()
            .Where(x => x.Reference == token)
            .Select(x => new
            {
                ClientIdFromGrant = (x as GrantToken)!.AuthorizationGrant.Client.Id,
                ClientId = (x as ClientAccessToken)!.Client.Id
            })
            .SingleOrDefaultAsync(cancellationToken: cancellationToken);

        return clientToken?.ClientIdFromGrant ?? clientToken?.ClientId;
    }
}