using AuthServer.Cache.Abstractions;
using AuthServer.Constants;
using AuthServer.Core;
using AuthServer.Core.Abstractions;
using AuthServer.Core.Request;
using AuthServer.EndSession.Abstractions;
using AuthServer.Entities;
using AuthServer.RequestAccessors.EndSession;
using AuthServer.TokenDecoders;
using AuthServer.TokenDecoders.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace AuthServer.EndSession;
internal class EndSessionRequestValidator : IRequestValidator<EndSessionRequest, EndSessionValidatedRequest>
{
    private readonly AuthorizationDbContext _authorizationDbContext;
    private readonly IEndSessionUserAccessor _endSessionUserAccessor;
    private readonly ITokenDecoder<ServerIssuedTokenDecodeArguments> _tokenDecoder;
    private readonly ICachedClientStore _cachedClientStore;

    public EndSessionRequestValidator(
        AuthorizationDbContext authorizationDbContext,
        IEndSessionUserAccessor endSessionUserAccessor,
        ITokenDecoder<ServerIssuedTokenDecodeArguments> tokenDecoder,
        ICachedClientStore cachedClientStore)
    {
        _authorizationDbContext = authorizationDbContext;
        _endSessionUserAccessor = endSessionUserAccessor;
        _tokenDecoder = tokenDecoder;
        _cachedClientStore = cachedClientStore;
    }

    public async Task<ProcessResult<EndSessionValidatedRequest, ProcessError>> Validate(EndSessionRequest request, CancellationToken cancellationToken)
    {
        var requiredParametersError = ValidateRequiredParameters(request);
        if (requiredParametersError is not null)
        {
            return requiredParametersError;
        }

        if (!string.IsNullOrEmpty(request.IdTokenHint))
        {
            return await ValidateRequestForIdTokenHint(request, cancellationToken);
        }
        else
        {
            return await ValidateRequestForInteraction(request, cancellationToken);
        }
    }

    private static ProcessError? ValidateRequiredParameters(EndSessionRequest request)
    {
        if (string.IsNullOrEmpty(request.PostLogoutRedirectUri)
            && !string.IsNullOrEmpty(request.State))
        {
            return EndSessionError.StateWithoutPostLogoutRedirectUri;
        }

        if (string.IsNullOrEmpty(request.State)
            && !string.IsNullOrEmpty(request.PostLogoutRedirectUri))
        {
            return EndSessionError.PostLogoutRedirectUriWithoutState;
        }

        if (string.IsNullOrEmpty(request.ClientId)
            && string.IsNullOrEmpty(request.IdTokenHint)
            && !string.IsNullOrEmpty(request.PostLogoutRedirectUri))
        {
            return EndSessionError.PostLogoutRedirectUriWithoutClientIdOrIdTokenHint;
        }

        return null;
    }

    private async Task<ProcessResult<EndSessionValidatedRequest, ProcessError>> ValidateRequestForIdTokenHint(EndSessionRequest request, CancellationToken cancellationToken)
    {
        var token = await _tokenDecoder.Validate(request.IdTokenHint!, new ServerIssuedTokenDecodeArguments
        {
            ValidateLifetime = false,
            TokenTypes = [TokenTypeHeaderConstants.IdToken],
            Audiences = string.IsNullOrEmpty(request.ClientId) ? [] : [request.ClientId]
        }, cancellationToken);

        if (token is null)
        {
            return EndSessionError.InvalidIdToken;
        }

        var audience = token.Audiences.Single();
        if (!string.IsNullOrEmpty(request.ClientId)
            && audience != request.ClientId)
        {
            return EndSessionError.MismatchingClientId;
        }

        var subjectIdentifier = token.Subject;
        var sessionId = token.GetClaim(ClaimNameConstants.Sid).Value;

        var unauthorizedClientError = await ValidateClientAuthorizedForPostLogoutRedirectUri(audience, request, cancellationToken);
        if (unauthorizedClientError is not null)
        {
            return unauthorizedClientError;
        }

        return new EndSessionValidatedRequest
        {
            SubjectIdentifier = subjectIdentifier,
            SessionId = sessionId,
            ClientId = audience,
            LogoutAtIdentityProvider = true
        };
    }

    private async Task<ProcessResult<EndSessionValidatedRequest, ProcessError>> ValidateRequestForInteraction(EndSessionRequest request, CancellationToken cancellationToken)
    {
        var endSessionUser = _endSessionUserAccessor.TryGetUser();
        if (endSessionUser is null)
        {
            return EndSessionError.InteractionRequired;
        }

        var subjectIdentifier = endSessionUser.SubjectIdentifier;
        var sessionId = await _authorizationDbContext
            .Set<Session>()
            .Where(s => s.SubjectIdentifier.Id == endSessionUser.SubjectIdentifier)
            .Where(Session.IsActive)
            .Select(s => s.Id)
            .SingleOrDefaultAsync(cancellationToken);

        var clientId = request.ClientId;

        if (string.IsNullOrEmpty(clientId))
        {
            return new EndSessionValidatedRequest
            {
                SubjectIdentifier = subjectIdentifier,
                SessionId = sessionId,
                ClientId = null,
                LogoutAtIdentityProvider = endSessionUser.LogoutAtIdentityProvider
            };
        }

        var unauthorizedClientError = await ValidateClientAuthorizedForPostLogoutRedirectUri(clientId, request, cancellationToken);
        if (unauthorizedClientError is not null)
        {
            return unauthorizedClientError;
        }

        return new EndSessionValidatedRequest
        {
            SubjectIdentifier = subjectIdentifier,
            SessionId = sessionId,
            ClientId = clientId,
            LogoutAtIdentityProvider = endSessionUser.LogoutAtIdentityProvider
        };
    }

    private async Task<ProcessError?> ValidateClientAuthorizedForPostLogoutRedirectUri(string clientId, EndSessionRequest request, CancellationToken cancellationToken)
    {
        var cachedClient = await _cachedClientStore.Get(clientId, cancellationToken);
        if (!string.IsNullOrEmpty(request.PostLogoutRedirectUri)
            && !cachedClient.PostLogoutRedirectUris.Contains(request.PostLogoutRedirectUri))
        {
            return EndSessionError.UnauthorizedClientForPostLogoutRedirectUri;
        }

        return null;
    }
}
