using System.Net;
using Domain;
using Domain.Constants;
using Infrastructure.Builders.Token.Abstractions;
using Infrastructure.Builders.Token.LogoutToken;
using Infrastructure.Decoders.Token;
using Infrastructure.Decoders.Token.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Requests.EndSession;

public class EndSessionHandler : IRequestHandler<EndSessionCommand, EndSessionResponse>
{
  private readonly IdentityContext _identityContext;
  private readonly IHttpClientFactory _httpClientFactory;
  private readonly ITokenBuilder<LogoutTokenArguments> _tokenBuilder;
  private readonly IStructuredTokenDecoder _tokenDecoder;

  public EndSessionHandler(
    IdentityContext identityContext,
    IHttpClientFactory httpClientFactory,
    ITokenBuilder<LogoutTokenArguments> tokenBuilder,
    IStructuredTokenDecoder tokenDecoder)
  {
    _identityContext = identityContext;
    _httpClientFactory = httpClientFactory;
    _tokenBuilder = tokenBuilder;
    _tokenDecoder = tokenDecoder;
  }

  public async Task<EndSessionResponse> Handle(EndSessionCommand request, CancellationToken cancellationToken)
  {
    var idToken = await _tokenDecoder.Decode(request.IdTokenHint, new StructuredTokenDecoderArguments
    {
      ClientId = request.ClientId,
    });

    var sessionId = idToken.Claims.Single(x => x.Type == ClaimNameConstants.Sid).Value;
    var userId = idToken.Claims.Single(x => x.Type == ClaimNameConstants.Sub).Value;
    var clients = await _identityContext
      .Set<Session>()
      .Where(x => x.Id == sessionId)
      .Where(x => !x.IsRevoked)
      .SelectMany(x => x.AuthorizationCodeGrants)
      .Select(x => x.Client)
      .Distinct()
      .Where(x => x.BackChannelLogoutUri != null)
      .Select(x => new
      {
        ClientId = x.Id,
        LogoutUri = x.BackChannelLogoutUri
      })
      .ToListAsync(cancellationToken: cancellationToken);

    await Parallel.ForEachAsync(clients, cancellationToken, async (client, requestCancellationToken) =>
    {
      using var httpClient = _httpClientFactory.CreateClient();
      var logoutToken = await _tokenBuilder.BuildToken(new LogoutTokenArguments
      {
        ClientId = request.ClientId,
        SessionId = sessionId,
        UserId = userId
      });
      var formBody = new Dictionary<string, string>
      {
        { "logout_token", logoutToken }
      };
      var httpRequest = new HttpRequestMessage(HttpMethod.Post, client.LogoutUri)
      {
        Content = new FormUrlEncodedContent(formBody)
      };
      await httpClient.SendAsync(httpRequest, cancellationToken: requestCancellationToken);
    });

    return GetOkResponse(request);
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