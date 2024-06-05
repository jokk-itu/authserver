using AuthServer.Cache;
using AuthServer.Codes;
using AuthServer.Core;
using AuthServer.Core.Abstractions;
using AuthServer.Core.RequestProcessing;
using AuthServer.Entities;
using AuthServer.Helpers;
using AuthServer.Repositories;
using AuthServer.Repositories.Abstract;
using AuthServer.RequestAccessors.Token;
using Microsoft.EntityFrameworkCore;

namespace AuthServer.TokenByGrant.AuthorizationCodeGrant;

internal class AuthorizationCodeValidator : IRequestValidator<TokenRequest, AuthorizationCodeValidatedRequest>
{
    private readonly AuthorizationDbContext _identityContext;
    private readonly IAuthorizationCodeEncoder _authorizationCodeEncoder;
    private readonly IClientAuthenticationService _clientAuthenticationService;
    private readonly IClientRepository _clientRepository;
    private readonly ICachedClientStore _cachedEntityStore;
    private readonly IConsentGrantRepository _consentGrantRepository;

    public AuthorizationCodeValidator(
        AuthorizationDbContext identityContext,
        IAuthorizationCodeEncoder authorizationCodeEncoder,
        IClientAuthenticationService clientAuthenticationService,
        IClientRepository clientRepository,
        ICachedClientStore cachedEntityStore,
        IConsentGrantRepository consentGrantRepository)
    {
        _identityContext = identityContext;
        _authorizationCodeEncoder = authorizationCodeEncoder;
        _clientAuthenticationService = clientAuthenticationService;
        _clientRepository = clientRepository;
        _cachedEntityStore = cachedEntityStore;
        _consentGrantRepository = consentGrantRepository;
    }
    
    public async Task<ProcessResult<AuthorizationCodeValidatedRequest, ProcessError>> Validate(TokenRequest request, CancellationToken cancellationToken)
    {
        if (request.Resource.Count == 0)
        {
            return TokenError.InvalidTarget;
        }

        var authorizationCode = _authorizationCodeEncoder.DecodeAuthorizationCode(request.Code);
        if (authorizationCode is null)
        {
            return TokenError.InvalidCode;
        }

        var isCodeVerifierInvalid = ProofKeyForCodeExchangeHelper.IsCodeVerifierValid(request.CodeVerifier, authorizationCode.CodeChallenge);
        if (isCodeVerifierInvalid)
        {
            return TokenError.InvalidCodeVerifier;
        }

        var isRedirectUriMismatch = !string.IsNullOrWhiteSpace(authorizationCode.RedirectUri)
                                    && request.RedirectUri != authorizationCode.RedirectUri;

        if (isRedirectUriMismatch)
        {
            return TokenError.InvalidRedirectUri;
        }

        var isClientAuthenticationMethodInvalid = request.ClientAuthentications.Count != 1;
        if (isClientAuthenticationMethodInvalid)
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

        var publicSubjectIdentifier = await _identityContext
            .Set<AuthorizationGrant>()
            .Where(x => x.Id == authorizationCode.AuthorizationGrantId)
            .Where(AuthorizationGrant.IsAuthorizationCodeValid(authorizationCode.AuthorizationCodeId))
            .Where(x => x.Session.RevokedAt == null)
            .Select(x => x.Session.PublicSubjectIdentifier.Id)
            .SingleOrDefaultAsync(cancellationToken: cancellationToken);

        if (publicSubjectIdentifier is null)
        {
            return TokenError.InvalidGrant;
        }

        var cachedClient = await _cachedEntityStore
            .Get(clientId, cancellationToken);

        if (cachedClient.GrantTypes.All(x => x != request.GrantType))
        {
            return TokenError.UnauthorizedClientForGrantType;
        }

        if (!string.IsNullOrWhiteSpace(request.RedirectUri)
            && cachedClient.RedirectUris.All(x => x != request.RedirectUri))
        {
            return TokenError.UnauthorizedClientForRedirectUri;
        }

        // Request.Scopes cannot be given during authorization_code grant
        var scope = authorizationCode.Scope;

        if (cachedClient.RequireConsent)
        {
            var consentedScopes = await _consentGrantRepository.GetConsentedScope(publicSubjectIdentifier, clientId, cancellationToken);
            if (consentedScopes.Count == 0)
            {
                return TokenError.ConsentRequired;
            }

            if (scope.Except(consentedScopes).Any())
            {
                return TokenError.ScopeExceedsConsentedScope;
            }
        }

        var doesResourcesExist = await _clientRepository.DoesResourcesExist(request.Resource, scope, cancellationToken);
        if (!doesResourcesExist)
        {
            return TokenError.InvalidTarget;
        }

        return new AuthorizationCodeValidatedRequest
        {
            AuthorizationGrantId = authorizationCode.AuthorizationGrantId,
            AuthorizationCodeId = authorizationCode.AuthorizationCodeId,
            Resource = request.Resource,
            Scope = scope
        };
    }
}