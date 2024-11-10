using AuthServer.Authentication;
using AuthServer.Authentication.Abstractions;
using AuthServer.Cache.Abstractions;
using AuthServer.Codes.Abstractions;
using AuthServer.Constants;
using AuthServer.Core;
using AuthServer.Core.Abstractions;
using AuthServer.Core.Request;
using AuthServer.Entities;
using AuthServer.Extensions;
using AuthServer.Helpers;
using AuthServer.Repositories.Abstractions;
using AuthServer.RequestAccessors.Token;
using Microsoft.EntityFrameworkCore;

namespace AuthServer.TokenByGrant.AuthorizationCodeGrant;

internal class AuthorizationCodeRequestValidator : IRequestValidator<TokenRequest, AuthorizationCodeValidatedRequest>
{
    private readonly AuthorizationDbContext _identityContext;
    private readonly IAuthorizationCodeEncoder _authorizationCodeEncoder;
    private readonly IClientAuthenticationService _clientAuthenticationService;
    private readonly IClientRepository _clientRepository;
    private readonly ICachedClientStore _cachedEntityStore;
    private readonly IConsentGrantRepository _consentGrantRepository;

    public AuthorizationCodeRequestValidator(
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
        if (request.GrantType != GrantTypeConstants.AuthorizationCode)
        {
            return TokenError.UnsupportedGrantType;
        }

        if (request.Resource.Count == 0)
        {
            return TokenError.InvalidTarget;
        }

        var authorizationCode = _authorizationCodeEncoder.DecodeAuthorizationCode(request.Code);
        if (authorizationCode is null)
        {
            return TokenError.InvalidCode;
        }

        var isCodeVerifierValid = ProofKeyForCodeExchangeHelper.IsCodeVerifierValid(request.CodeVerifier, authorizationCode.CodeChallenge);
        if (!isCodeVerifierValid)
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

        var subjectIdentifier = await _identityContext
            .Set<AuthorizationGrant>()
            .Where(AuthorizationGrant.IsActive)
            .Where(x => x.Id == authorizationCode.AuthorizationGrantId)
            .Where(x => x.AuthorizationCodes
                .AsQueryable()
                .Where(y => y.Id == authorizationCode.AuthorizationCodeId)
                .Where(y => y.RedeemedAt == null)
                .Any(y => y.ExpiresAt > DateTime.UtcNow))
            .Select(x => x.Session.SubjectIdentifier.Id)
            .SingleOrDefaultAsync(cancellationToken: cancellationToken);

        if (subjectIdentifier is null)
        {
            return TokenError.InvalidGrant;
        }

        var clientId = clientAuthenticationResult.ClientId!;
        var cachedClient = await _cachedEntityStore.Get(clientId, cancellationToken);

        if (cachedClient.GrantTypes.All(x => x != request.GrantType))
        {
            return TokenError.UnauthorizedForGrantType;
        }

        if (!string.IsNullOrWhiteSpace(request.RedirectUri)
            && cachedClient.RedirectUris.All(x => x != request.RedirectUri))
        {
            return TokenError.UnauthorizedForRedirectUri;
        }

        // Request.Scopes cannot be given during authorization_code grant
        var scope = authorizationCode.Scope;

        // Check scope again, as the authorized scope can change
        if (cachedClient.Scopes.ExceptAny(scope))
        {
            return TokenError.UnauthorizedForScope;
        }

        if (cachedClient.RequireConsent)
        {
            var consentedScopes = await _consentGrantRepository.GetConsentedScope(subjectIdentifier, clientId, cancellationToken);
            if (consentedScopes.Count == 0)
            {
                return TokenError.ConsentRequired;
            }

            if (scope.ExceptAny(consentedScopes))
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