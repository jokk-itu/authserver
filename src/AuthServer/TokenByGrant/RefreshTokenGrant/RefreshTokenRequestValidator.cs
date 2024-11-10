using AuthServer.Authentication;
using AuthServer.Authentication.Abstractions;
using AuthServer.Cache.Abstractions;
using AuthServer.Constants;
using AuthServer.Core;
using AuthServer.Core.Abstractions;
using AuthServer.Core.Request;
using AuthServer.Entities;
using AuthServer.Extensions;
using AuthServer.Helpers;
using AuthServer.Repositories.Abstractions;
using AuthServer.RequestAccessors.Token;
using AuthServer.TokenDecoders;
using AuthServer.TokenDecoders.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace AuthServer.TokenByGrant.RefreshTokenGrant;

internal class RefreshTokenRequestValidator : IRequestValidator<TokenRequest, RefreshTokenValidatedRequest>
{
    private readonly AuthorizationDbContext _identityContext;
    private readonly ITokenDecoder<ServerIssuedTokenDecodeArguments> _tokenDecoder;
    private readonly IClientAuthenticationService _clientAuthenticationService;
    private readonly ICachedClientStore _cachedClientStore;
    private readonly IClientRepository _clientRepository;
    private readonly IConsentGrantRepository _consentGrantRepository;

    public RefreshTokenRequestValidator(
        AuthorizationDbContext identityContext,
        ITokenDecoder<ServerIssuedTokenDecodeArguments> tokenDecoder,
        IClientAuthenticationService clientAuthenticationService,
        ICachedClientStore cachedClientStore,
        IClientRepository clientRepository,
        IConsentGrantRepository consentGrantRepository)
    {
        _identityContext = identityContext;
        _tokenDecoder = tokenDecoder;
        _clientAuthenticationService = clientAuthenticationService;
        _cachedClientStore = cachedClientStore;
        _clientRepository = clientRepository;
        _consentGrantRepository = consentGrantRepository;
    }

    public async Task<ProcessResult<RefreshTokenValidatedRequest, ProcessError>> Validate(TokenRequest request, CancellationToken cancellationToken)
    {
        if (request.GrantType != GrantTypeConstants.RefreshToken)
        {
            return TokenError.UnsupportedGrantType;
        }

        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return TokenError.InvalidRefreshToken;
        }

        if (request.Resource.Count == 0)
        {
            return TokenError.InvalidTarget;
        }

        if (request.ClientAuthentications.Count != 1)
        {
            return TokenError.MultipleOrNoneClientMethod;
        }

        var clientAuthentication = request.ClientAuthentications.Single();
        var clientAuthenticationResult = await _clientAuthenticationService.AuthenticateClient(clientAuthentication, cancellationToken);
        if (!clientAuthenticationResult.IsAuthenticated || string.IsNullOrWhiteSpace(clientAuthenticationResult.ClientId))
        {
            return TokenError.InvalidClient;
        }
        
        var clientId = clientAuthenticationResult.ClientId!;
        string? authorizationGrantId;
        if (TokenHelper.IsJsonWebToken(request.RefreshToken))
        {
            authorizationGrantId = await ValidateStructuredToken(clientId, request.RefreshToken, cancellationToken);
        }
        else
        {
            authorizationGrantId = await ValidateReferenceToken(request.RefreshToken, cancellationToken);
        }

        if (authorizationGrantId is null)
        {
            return TokenError.InvalidRefreshToken;
        }

        var cachedClient = await _cachedClientStore.Get(clientId, cancellationToken);

        if (cachedClient.GrantTypes.All(x => x != GrantTypeConstants.RefreshToken))
        {
            return TokenError.UnauthorizedForGrantType;
        }

        /*
         * Do not check for validity of grant,
         * as the grant and session must be active,
         * if the RefreshToken is active.
         */
        var subjectIdentifier = await _identityContext
            .Set<AuthorizationGrant>()
            .Where(x => x.Id == authorizationGrantId)
            .Select(x => x.Session.SubjectIdentifier.Id)
            .SingleAsync(cancellationToken);

        IReadOnlyCollection<string> requestedScope;
        var isScopeRequested = request.Scope.Count != 0;

        if (cachedClient.RequireConsent)
        {
            var consentedScope = await _consentGrantRepository.GetConsentedScope(
                subjectIdentifier, clientId, cancellationToken);

            if (consentedScope.Count == 0)
            {
                return TokenError.ConsentRequired;
            }

            requestedScope = isScopeRequested ? request.Scope : consentedScope;
            if (requestedScope.ExceptAny(consentedScope))
            {
                return TokenError.ScopeExceedsConsentedScope;
            }
        }
        else
        {
            requestedScope = isScopeRequested ? request.Scope : cachedClient.Scopes;
            if (requestedScope.ExceptAny(cachedClient.Scopes))
            {
                return TokenError.UnauthorizedForScope;
            }
        }

        var doesResourceExist = await _clientRepository.DoesResourcesExist(
            request.Resource, requestedScope, cancellationToken);

        if (!doesResourceExist)
        {
            return TokenError.InvalidTarget;
        }

        return new RefreshTokenValidatedRequest
        {
            AuthorizationGrantId = authorizationGrantId,
            ClientId = clientId,
            Resource = request.Resource,
            Scope = requestedScope
        };
    }

    private async Task<string?> ValidateReferenceToken(string refreshToken, CancellationToken cancellationToken)
    {
        var authorizationGrantId = await _identityContext
            .Set<RefreshToken>()
            .Where(x => x.Reference == refreshToken)
            .Where(Token.IsActive)
            .OfType<RefreshToken>()
            .Select(x => x.AuthorizationGrant.Id)
            .SingleOrDefaultAsync(cancellationToken: cancellationToken);

        return authorizationGrantId;
    }

    private async Task<string?> ValidateStructuredToken(string clientId, string refreshToken, CancellationToken cancellationToken)
    {
        var validatedToken = await _tokenDecoder.Validate(refreshToken, new ServerIssuedTokenDecodeArguments
        {
            ValidateLifetime = true,
            Audiences = [clientId],
            TokenTypes = [TokenTypeHeaderConstants.RefreshToken]
        }, cancellationToken);

        if (validatedToken is null)
        {
            return null;
        }

        var authorizationGrantId = validatedToken.Claims.Single(x => x.Type == ClaimNameConstants.GrantId).Value;
        var jti = Guid.Parse(validatedToken.Claims.Single(x => x.Type == ClaimNameConstants.Jti).Value);

        var isActive = await _identityContext
            .Set<RefreshToken>()
            .Where(x => x.Id == jti)
            .Where(Token.IsActive)
            .AnyAsync(cancellationToken: cancellationToken);

        return isActive ? authorizationGrantId : null;
    }
}