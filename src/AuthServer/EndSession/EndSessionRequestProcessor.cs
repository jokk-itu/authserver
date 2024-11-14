using AuthServer.Authentication.Abstractions;
using AuthServer.Core;
using AuthServer.Core.Abstractions;
using AuthServer.Core.Request;
using AuthServer.Entities;
using AuthServer.Repositories.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AuthServer.EndSession;

internal class EndSessionRequestProcessor : IRequestProcessor<EndSessionValidatedRequest, Unit>
{
    private readonly AuthorizationDbContext _authorizationDbContext;
    private readonly ILogger<EndSessionRequestProcessor> _logger;
    private readonly IClientLogoutService _clientLogoutService;
    private readonly ISessionRepository _sessionRepository;
    private readonly IAuthorizationGrantRepository _authorizationGrantRepository;

    public EndSessionRequestProcessor(
        AuthorizationDbContext authorizationDbContext,
        ILogger<EndSessionRequestProcessor> logger,
        IClientLogoutService clientLogoutService,
        ISessionRepository sessionRepository,
        IAuthorizationGrantRepository authorizationGrantRepository)
    {
        _authorizationDbContext = authorizationDbContext;
        _logger = logger;
        _clientLogoutService = clientLogoutService;
        _sessionRepository = sessionRepository;
        _authorizationGrantRepository = authorizationGrantRepository;
    }

    public async Task<Unit> Process(EndSessionValidatedRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(request.SessionId))
        {
            _logger.LogDebug("Session is inactive, nothing to logout from");
            return Unit.Value;
        }

        var clients = new List<Client>();

        if (request.LogoutAtIdentityProvider)
        {
            var sessionQuery = await _authorizationDbContext
                .Set<Session>()
                .Where(Session.IsActive)
                .Where(s => s.Id == request.SessionId)
                .Select(s => new SessionQuery(
                    s.AuthorizationGrants
                        .AsQueryable()
                        .Where(AuthorizationGrant.IsActive)
                        .Select(ag => ag.Client)
                        .ToList())
                )
                .SingleAsync(cancellationToken);

            await _sessionRepository.RevokeSession(request.SessionId, cancellationToken);
            clients = sessionQuery.Clients.ToList();
        }
        else if (!string.IsNullOrEmpty(request.ClientId))
        {
            var authorizationGrantQuery = await _authorizationDbContext
                .Set<AuthorizationGrant>()
                .Where(AuthorizationGrant.IsActive)
                .Where(ag => ag.Client.Id == request.ClientId)
                .Where(ag => ag.Session.Id == request.SessionId)
                .Select(ag => new AuthorizationGrantQuery(ag.Id, ag.Client))
                .SingleOrDefaultAsync(cancellationToken);

            if (authorizationGrantQuery is null)
            {
                _logger.LogDebug(
                    "AuthorizationGrant from Session {SessionId} and Client {ClientId} is not active",
                    request.SessionId,
                    request.ClientId);

                return Unit.Value;
            }

            await _authorizationGrantRepository.RevokeGrant(authorizationGrantQuery.AuthorizationGrantId, cancellationToken);
            clients.Add(authorizationGrantQuery.Client);
        }
        else
        {
            _logger.LogDebug(
                "Not logging out from IdP and no client is provided for session {SessionId}",
                request.SessionId);

            return Unit.Value;
        }

        await BackchannelLogout(clients, request, cancellationToken);
        return Unit.Value;
    }

    private async Task BackchannelLogout(IEnumerable<Client> clients, EndSessionValidatedRequest request,
        CancellationToken cancellationToken)
    {
        foreach (var client in clients.Where(x => x.BackchannelLogoutUri is not null))
        {
            await _clientLogoutService.Logout(client.Id, request.SessionId, request.SubjectIdentifier, cancellationToken);
        }
    }

    private sealed record SessionQuery(IEnumerable<Client> Clients);

    private sealed record AuthorizationGrantQuery(string AuthorizationGrantId, Client Client);
}