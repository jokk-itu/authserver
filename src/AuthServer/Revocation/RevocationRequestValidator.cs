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
using Microsoft.Extensions.Logging;

namespace AuthServer.Revocation;
internal class RevocationRequestValidator : IRequestValidator<RevocationRequest, RevocationValidatedRequest>
{
    private readonly AuthorizationDbContext _identityContext;
    private readonly ITokenDecoder<ServerIssuedTokenDecodeArguments> _serverIssuedTokenDecoder;
    private readonly IClientAuthenticationService _clientAuthenticationService;
    private readonly ILogger<RevocationRequestValidator> _logger;

    public RevocationRequestValidator(
        AuthorizationDbContext identityContext,
        ITokenDecoder<ServerIssuedTokenDecodeArguments> serverIssuedTokenDecoder,
        IClientAuthenticationService clientAuthenticationService,
        ILogger<RevocationRequestValidator> logger)
    {
        _identityContext = identityContext;
        _serverIssuedTokenDecoder = serverIssuedTokenDecoder;
        _clientAuthenticationService = clientAuthenticationService;
        _logger = logger;
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
        if (TokenHelper.IsStructuredToken(request.Token))
        {
            clientIdFromToken = await GetClientIdFromStructuredToken(request.Token, cancellationToken);
        }
        else
        {
            clientIdFromToken = await GetClientIdFromReferenceToken(request.Token, cancellationToken);
        }

        if (!string.IsNullOrWhiteSpace(clientIdFromToken) && clientAuthenticationResult.ClientId != clientIdFromToken)
        {
            return RevocationError.ClientIdDoesNotMatchToken;
        }

        return new RevocationValidatedRequest
        {
            Token = request.Token
        };
    }

    private async Task<string?> GetClientIdFromStructuredToken(string token, CancellationToken cancellationToken)
    {
        try
        {
            var securityToken = await _serverIssuedTokenDecoder.Validate(token, new ServerIssuedTokenDecodeArguments
            {
                ValidateLifetime = false,
                Audiences = [],
                TokenTypes = [TokenTypeHeaderConstants.AccessToken, TokenTypeHeaderConstants.RefreshToken]
            }, cancellationToken);

            securityToken.TryGetClaim(ClaimNameConstants.ClientId, out var claim);
            return claim?.Value;
        }
        catch (Exception e)
        {
            _logger.LogWarning(e, "Extracting clientId failed");

            // If the token is invalid, the toke is ignored per rfc 7009
            return null;
        }
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