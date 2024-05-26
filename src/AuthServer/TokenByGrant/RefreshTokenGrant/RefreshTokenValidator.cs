using AuthServer.Cache;
using AuthServer.Constants;
using AuthServer.Core;
using AuthServer.Core.Abstractions;
using AuthServer.Core.RequestProcessing;
using AuthServer.Entities;
using AuthServer.Helpers;
using AuthServer.Repositories;
using AuthServer.Repositories.Abstract;
using AuthServer.RequestAccessors.Token;
using AuthServer.TokenDecoders;
using Microsoft.EntityFrameworkCore;

namespace AuthServer.TokenByGrant.RefreshTokenGrant;

internal class RefreshTokenValidator : IRequestValidator<TokenRequest, RefreshTokenValidatedRequest>
{
    private readonly IdentityContext _identityContext;
    private readonly ITokenDecoder<ServerIssuedTokenDecodeArguments> _tokenDecoder;
    private readonly IClientAuthenticationService _clientAuthenticationService;
    private readonly ICachedClientStore _cachedClientStore;
    private readonly IClientRepository _clientRepository;
    private readonly IConsentGrantRepository _consentGrantRepository;

    public RefreshTokenValidator(
        IdentityContext identityContext,
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
        if (clientAuthenticationResult.IsAuthenticated ||
            string.IsNullOrWhiteSpace(clientAuthenticationResult.ClientId))
        {
            return TokenError.InvalidClient;
        }
        
        var clientId = clientAuthenticationResult.ClientId!;
        string? authorizationGrantId;
        if (TokenHelper.IsStructuredToken(request.RefreshToken))
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

        var subjectIdentifier = await _identityContext
            .Set<AuthorizationGrant>()
            .Where(AuthorizationGrant.IsMaxAgeValid)
            .Select(x => x.Session.PublicSubjectIdentifier.Id)
            .SingleOrDefaultAsync(cancellationToken);

        if (subjectIdentifier is null)
        {
            return TokenError.LoginRequired;
        }

        var cachedClient = await _cachedClientStore.Get(clientId, cancellationToken);

        if (cachedClient.GrantTypes.All(x => x != GrantTypeConstants.RefreshToken))
        {
            return TokenError.UnauthorizedClientForGrantType;
        }

        IReadOnlyCollection<string> requestedScope;
        IEnumerable<string> requestComparableScope;
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
            requestComparableScope = consentedScope;
        }
        else
        {
            requestedScope = isScopeRequested ? request.Scope : cachedClient.Scopes;
            requestComparableScope = cachedClient.Scopes;
        }

        var isRequestedScopeInvalid = isScopeRequested && request.Scope.Except(requestComparableScope).Any();
        if (isRequestedScopeInvalid)
        {
            return TokenError.ScopeExceedsConsentedScope;
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
        try
        {
            var token = await _tokenDecoder.Validate(refreshToken, new ServerIssuedTokenDecodeArguments
            {
                ValidateLifetime = true,
                Audiences = [ clientId ],
                TokenTypes = [ TokenTypeHeaderConstants.RefreshToken ]
            }, cancellationToken);

            var authorizationGrantId = token.Claims.Single(x => x.Type == ClaimNameConstants.GrantId).Value;
            var jti = Guid.Parse(token.Claims.Single(x => x.Type == ClaimNameConstants.Jti).Value);

            var isRevoked = await _identityContext
                .Set<RefreshToken>()
                .Where(x => x.Id == jti)
                .Where(Token.IsActive)
                .AnyAsync(cancellationToken: cancellationToken);

            return isRevoked ? null : authorizationGrantId;
        }
        catch (Exception)
        {
            return null;
        }
    }
}