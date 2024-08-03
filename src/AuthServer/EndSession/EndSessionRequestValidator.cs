using AuthServer.Cache.Abstractions;
using AuthServer.Constants;
using AuthServer.Core;
using AuthServer.Core.Request;
using AuthServer.EndSession.Abstractions;
using AuthServer.Entities;
using AuthServer.RequestAccessors.EndSession;
using AuthServer.TokenDecoders;
using AuthServer.TokenDecoders.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AuthServer.EndSession;
internal class EndSessionRequestValidator : IRequestValidator<EndSessionRequest, EndSessionValidatedRequest>
{
    private readonly AuthorizationDbContext _authorizationDbContext;
    private readonly IEndSessionUserAccessor _endSessionUserAccessor;
    private readonly ITokenDecoder<ServerIssuedTokenDecodeArguments> _tokenDecoder;
    private readonly ICachedClientStore _cachedClientStore;
    private readonly ILogger<EndSessionRequestValidator> _logger;

    public EndSessionRequestValidator(
        AuthorizationDbContext authorizationDbContext,
        IEndSessionUserAccessor endSessionUserAccessor,
        ITokenDecoder<ServerIssuedTokenDecodeArguments> tokenDecoder,
        ICachedClientStore cachedClientStore,
        ILogger<EndSessionRequestValidator> logger)
    {
        _authorizationDbContext = authorizationDbContext;
        _endSessionUserAccessor = endSessionUserAccessor;
        _tokenDecoder = tokenDecoder;
        _cachedClientStore = cachedClientStore;
        _logger = logger;
    }

    public async Task<ProcessResult<EndSessionValidatedRequest, ProcessError>> Validate(EndSessionRequest request, CancellationToken cancellationToken)
    {
        var endSessionUser = _endSessionUserAccessor.TryGetUser();
        if (endSessionUser is null)
        {
            return EndSessionError.InteractionRequired;
        }

        string? subjectIdentifier = null;
        string? sessionId = null;
        string? clientId = null;

        if (!string.IsNullOrEmpty(endSessionUser.SubjectIdentifier))
        {
            subjectIdentifier = endSessionUser.SubjectIdentifier;
            sessionId = await _authorizationDbContext
                .Set<Session>()
                .Where(s => s.PublicSubjectIdentifier.Id == endSessionUser.SubjectIdentifier)
                .Where(Session.IsActive)
                .Select(s => s.Id)
                .SingleOrDefaultAsync(cancellationToken);

            clientId = request.ClientId;
        }
        else if (!string.IsNullOrEmpty(request.IdTokenHint))
        {
            try
            {
                var token = await _tokenDecoder.Validate(request.IdTokenHint, new ServerIssuedTokenDecodeArguments
                {
                    ValidateLifetime = false,
                    TokenTypes = [TokenTypeHeaderConstants.IdToken],
                    Audiences = string.IsNullOrEmpty(request.ClientId) ? [] : [request.ClientId]
                }, cancellationToken);

                subjectIdentifier = token.Subject;
                sessionId = token.GetClaim(ClaimNameConstants.Sid).Value;
                if (string.IsNullOrEmpty(clientId))
                {
                    clientId = token.GetClaim(ClaimNameConstants.ClientId).Value;
                }
            }
            catch (Exception e)
            {
                _logger.LogInformation(e, "IdTokenHint validation failed");
                return EndSessionError.InvalidIdToken;
            }
        }

        var hasEmptyPostLogoutRedirectUri = string.IsNullOrEmpty(request.PostLogoutRedirectUri);
        if (!hasEmptyPostLogoutRedirectUri && string.IsNullOrEmpty(clientId))
        {
            return EndSessionError.InvalidClientId;
        }

        if (!hasEmptyPostLogoutRedirectUri && string.IsNullOrEmpty(request.State))
        {
            return EndSessionError.InvalidState;
        }

        if (!string.IsNullOrEmpty(clientId))
        {
            var cachedClient = await _cachedClientStore.TryGet(clientId, cancellationToken);
            if (cachedClient is null)
            {
                return EndSessionError.InvalidClientId;
            }

            if (!hasEmptyPostLogoutRedirectUri && !cachedClient.PostLogoutRedirectUris.Contains(request.PostLogoutRedirectUri))
            {
                return EndSessionError.InvalidPostLogoutRedirectUri;
            }
        }

        return new EndSessionValidatedRequest
        {
            SubjectIdentifier = subjectIdentifier,
            SessionId = sessionId,
            ClientId = clientId,
            LogoutAtIdentityProvider = endSessionUser.LogoutAtIdentityProvider
        };
    }
}
