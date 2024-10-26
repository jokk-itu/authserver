using AuthServer.Core;
using AuthServer.Core.Request;
using AuthServer.Entities;
using AuthServer.TokenBuilders;
using AuthServer.TokenBuilders.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AuthServer.EndSession;
internal class EndSessionRequestProcessor : IRequestProcessor<EndSessionValidatedRequest, Unit>
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly AuthorizationDbContext _authorizationDbContext;
    private readonly ITokenBuilder<LogoutTokenArguments> _tokenBuilder;
    private readonly ILogger<EndSessionRequestProcessor> _logger;

    public EndSessionRequestProcessor(
        IHttpClientFactory httpClientFactory,
        AuthorizationDbContext authorizationDbContext,
        ITokenBuilder<LogoutTokenArguments> tokenBuilder,
        ILogger<EndSessionRequestProcessor> logger)
    {
        _httpClientFactory = httpClientFactory;
        _authorizationDbContext = authorizationDbContext;
        _tokenBuilder = tokenBuilder;
        _logger = logger;
    }

    public async Task<Unit> Process(EndSessionValidatedRequest request, CancellationToken cancellationToken)
    {
        // if sessionId is null, then nothing is left to revoke or logout
        if (string.IsNullOrEmpty(request.SessionId))
        {
            return Unit.Value;
        }

        var clients = new List<Client>();

        if (request.LogoutAtIdentityProvider)
        {
            var sessionQuery = await _authorizationDbContext
                .Set<Session>()
                .Where(Session.IsActive)
                .Where(s => s.Id == request.SessionId)
                .Select(s => new SessionQuery(s, s.AuthorizationGrants.Select(ag => ag.Client)))
                .SingleOrDefaultAsync(cancellationToken);

            if (sessionQuery is null)
            {
                _logger.LogDebug("Session {SessionId} is not active", request.SessionId);
                return Unit.Value;
            }

            await RevokeSessionRecursively(sessionQuery, cancellationToken);
            clients = sessionQuery.Clients.ToList();
        }
        else if (!string.IsNullOrEmpty(request.ClientId))
        {
            var authorizationGrantQuery = await _authorizationDbContext
                .Set<AuthorizationGrant>()
                .Where(AuthorizationGrant.IsActive)
                .Where(ag => ag.Client.Id == request.ClientId)
                .Where(ag => ag.Session.Id == request.SessionId)
                .Select(ag => new AuthorizationGrantQuery(ag, ag.Client))
                .SingleOrDefaultAsync(cancellationToken);

            if (authorizationGrantQuery is null)
            {
                _logger.LogDebug(
                    "AuthorizationGrant from Session {SessionId} and Client {ClientId} is not active",
                    request.SessionId,
                    request.ClientId);

                return Unit.Value;
            }

            await RevokeGrantRecursively(authorizationGrantQuery, cancellationToken);
            clients.Add(authorizationGrantQuery.Client);
        }
        else
        {
            // logout cannot happen, as no grants can be queried
            return Unit.Value;
        }

        await BackchannelLogout(clients, request, cancellationToken);
        return Unit.Value;
    }

    private async Task RevokeGrantRecursively(AuthorizationGrantQuery authorizationGrantQuery, CancellationToken cancellationToken)
    {
        authorizationGrantQuery.AuthorizationGrant.Revoke();
        var affectedTokens = await _authorizationDbContext
            .Set<AuthorizationGrant>()
            .Where(ag => ag.Id == authorizationGrantQuery.AuthorizationGrant.Id)
            .SelectMany(g => g.GrantTokens)
            .Where(Token.IsActive)
            .ExecuteUpdateAsync(
                propertyCall => propertyCall.SetProperty(gt => gt.RevokedAt, DateTime.UtcNow),
                cancellationToken);

        _logger.LogInformation(
            "Revoked AuthorizationGrant {AuthorizationGrantId} and Tokens {AffectedTokens}",
            authorizationGrantQuery.AuthorizationGrant.Id,
            affectedTokens);
    }

    private async Task RevokeSessionRecursively(SessionQuery sessionQuery, CancellationToken cancellationToken)
    {
        sessionQuery.Session.Revoke();
        var affectedGrants = await _authorizationDbContext
            .Set<AuthorizationGrant>()
            .Where(g => g.Session.Id == sessionQuery.Session.Id)
            .Where(AuthorizationGrant.IsActive)
            .ExecuteUpdateAsync(
                propertyCall => propertyCall.SetProperty(g => g.RevokedAt, DateTime.UtcNow),
                cancellationToken);

        var affectedTokens = await _authorizationDbContext
            .Set<AuthorizationGrant>()
            .Where(ag => ag.Session.Id == sessionQuery.Session.Id)
            .Where(AuthorizationGrant.IsActive)
            .SelectMany(g => g.GrantTokens)
            .Where(Token.IsActive)
            .ExecuteUpdateAsync(
                propertyCall => propertyCall.SetProperty(gt => gt.RevokedAt, DateTime.UtcNow),
                cancellationToken);

        _logger.LogInformation(
            "Revoked Session {SessionId}, AuthorizationGrants {AffectedGrants} and Tokens {AffectedTokens}",
            sessionQuery.Session.Id,
            affectedGrants,
            affectedTokens);
    }

    private async Task BackchannelLogout(IEnumerable<Client> clients, EndSessionValidatedRequest request, CancellationToken cancellationToken)
    {
        try
        {
            await Parallel.ForEachAsync(clients.Where(x => x.BackchannelLogoutUri != null), cancellationToken, async (client, innerCancellationToken) =>
            {
                var httpClient = _httpClientFactory.CreateClient(HttpClientNameConstants.Client);

                var logoutToken = await _tokenBuilder.BuildToken(new LogoutTokenArguments
                {
                    ClientId = client.Id,
                    SessionId = request.SessionId,
                    SubjectIdentifier = request.SubjectIdentifier
                }, innerCancellationToken);

                var body = new Dictionary<string, string>
                {
                    { Parameter.LogoutToken, logoutToken }
                };

                var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, client.BackchannelLogoutUri)
                {
                    Content = new FormUrlEncodedContent(body)
                };

                var response = await httpClient.SendAsync(httpRequestMessage, cancellationToken);
                response.EnsureSuccessStatusCode();
            });
        }
        catch (AggregateException e)
        {
            foreach (var innerException in e.InnerExceptions)
            {
                _logger.LogWarning(innerException, "Unexpected error during backchannel logout");
            }
        }
    }

    private sealed record SessionQuery(Session Session, IEnumerable<Client> Clients);

    private sealed record AuthorizationGrantQuery(AuthorizationGrant AuthorizationGrant, Client Client);
}