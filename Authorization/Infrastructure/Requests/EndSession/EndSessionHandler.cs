using System.Net;
using Domain;
using Domain.Constants;
using Infrastructure.Builders.Token.Abstractions;
using Infrastructure.Builders.Token.LogoutToken;
using Infrastructure.Decoders.Token;
using Infrastructure.Decoders.Token.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Requests.EndSession;

public class EndSessionHandler : IRequestHandler<EndSessionCommand, EndSessionResponse>
{
  private readonly IdentityContext _identityContext;
  private readonly IHttpClientFactory _httpClientFactory;
  private readonly ITokenBuilder<LogoutTokenArguments> _tokenBuilder;
  private readonly IStructuredTokenDecoder _tokenDecoder;
  private readonly ILogger<EndSessionHandler> _logger;

  public EndSessionHandler(
    IdentityContext identityContext,
    IHttpClientFactory httpClientFactory,
    ITokenBuilder<LogoutTokenArguments> tokenBuilder,
    IStructuredTokenDecoder tokenDecoder,
    ILogger<EndSessionHandler> logger)
  {
    _identityContext = identityContext;
    _httpClientFactory = httpClientFactory;
    _tokenBuilder = tokenBuilder;
    _tokenDecoder = tokenDecoder;
    _logger = logger;
  }

  public async Task<EndSessionResponse> Handle(EndSessionCommand request, CancellationToken cancellationToken)
  {
    var idToken = await _tokenDecoder.Decode(request.IdTokenHint, new StructuredTokenDecoderArguments
    {
      ClientId = request.ClientId
    });

    var sessionId = idToken.Claims.Single(x => x.Type == ClaimNameConstants.Sid).Value;
    var userId = idToken.Claims.Single(x => x.Type == ClaimNameConstants.Sub).Value;

    var session = await _identityContext
      .Set<Session>()
      .Where(x => x.Id == sessionId)
      .Where(x => !x.IsRevoked)
      .Include(x => x.AuthorizationCodeGrants)
      .ThenInclude(x => x.Client)
      .Include(x => x.AuthorizationCodeGrants)
      .ThenInclude(x => x.GrantTokens)
      .SingleOrDefaultAsync(cancellationToken: cancellationToken);

    if (session is null)
    {
      return GetOkResponse(request);
    }

    session.IsRevoked = true;
    foreach (var grant in session.AuthorizationCodeGrants)
    {
      grant.IsRevoked = true;
      foreach (var token in grant.GrantTokens)
      {
        token.RevokedAt = DateTime.UtcNow;
      }
    }
    await _identityContext.SaveChangesAsync(cancellationToken: cancellationToken);

    var clients = session.AuthorizationCodeGrants
      .Select(x => x.Client)
      .Distinct()
      .Where(x => x.BackchannelLogoutUri != null)
      .Select(x => new
      {
        ClientId = x.Id,
        LogoutUri = x.BackchannelLogoutUri!
      })
      .ToList();

    if (!clients.Any())
    {
      return GetOkResponse(request);
    }

    await Parallel.ForEachAsync(clients, cancellationToken, async (client, requestCancellationToken) =>
    {
      try
      {
        await LogoutClient(client.ClientId, sessionId, userId, client.LogoutUri, requestCancellationToken);
      }
      catch (Exception e)
      {
        _logger.LogWarning(e, "Backchannel logout to client {clientId} failed", client.ClientId);
      }
    });

    return GetOkResponse(request);
  }

  private async Task LogoutClient(string clientId, string sessionId, string userId, string logoutUri,
    CancellationToken cancellationToken)
  {
    using var httpClient = _httpClientFactory.CreateClient();
    var logoutToken = await _tokenBuilder.BuildToken(new LogoutTokenArguments
    {
      ClientId = clientId,
      SessionId = sessionId,
      UserId = userId
    });
    var formBody = new Dictionary<string, string>
    {
      { "logout_token", logoutToken }
    };
    var httpRequest = new HttpRequestMessage(HttpMethod.Post, logoutUri)
    {
      Content = new FormUrlEncodedContent(formBody)
    };
    await httpClient.SendAsync(httpRequest, cancellationToken: cancellationToken);
  }

  private static EndSessionResponse GetOkResponse(EndSessionCommand command)
  {
    if (string.IsNullOrWhiteSpace(command.PostLogoutRedirectUri))
    {
      return new EndSessionResponse(HttpStatusCode.OK);
    }

    return new EndSessionResponse(HttpStatusCode.Redirect);
  }
}