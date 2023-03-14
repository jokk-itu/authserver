using System.Net;
using Application;
using Domain;
using Infrastructure.Builders.Abstractions;
using Infrastructure.Decoders.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Requests.RedeemAuthorizationCodeGrant;
public class RedeemAuthorizationCodeGrantHandler : IRequestHandler<RedeemAuthorizationCodeGrantCommand, RedeemAuthorizationCodeGrantResponse>
{
  private readonly IdentityContext _identityContext;
  private readonly ITokenBuilder _tokenBuilder;
  private readonly IdentityConfiguration _identityConfiguration;
  private readonly ICodeDecoder _codeDecoder;

  public RedeemAuthorizationCodeGrantHandler(
    IdentityContext identityContext,
    ITokenBuilder tokenBuilder,
    IdentityConfiguration identityConfiguration,
    ICodeDecoder codeDecoder)
  {
    _identityContext = identityContext;
    _tokenBuilder = tokenBuilder;
    _identityConfiguration = identityConfiguration;
    _codeDecoder = codeDecoder;
  }

  public async Task<RedeemAuthorizationCodeGrantResponse> Handle(RedeemAuthorizationCodeGrantCommand request, CancellationToken cancellationToken)
  {
    var code = _codeDecoder.DecodeAuthorizationCode(request.Code);

    var query = await _identityContext
      .Set<AuthorizationCodeGrant>()
      .Where(x => x.Id == code.AuthorizationGrantId)
      .Select(x => new
      {
        AuthorizationCodeGrant = x,
        SessionId = x.Session.Id.ToString(),
        UserId = x.Session.User.Id,
        AuthorizationCode = x.AuthorizationCodes.Single(y => y.Id == code.AuthorizationCodeId),
        Nonce = x.Nonces.Single(y => y.Id == code.NonceId)
      })
      .SingleAsync(cancellationToken: cancellationToken);

    query.AuthorizationCode.IsRedeemed = true;
    query.AuthorizationCode.RedeemedAt = DateTime.UtcNow;

    var accessToken = await _tokenBuilder.BuildAccessToken(request.ClientId, code.Scopes, query.UserId, query.SessionId, cancellationToken: cancellationToken);
    var refreshToken = await _tokenBuilder.BuildRefreshToken(code.AuthorizationGrantId, request.ClientId, code.Scopes, query.UserId, query.SessionId, cancellationToken: cancellationToken);
    var idToken = await _tokenBuilder.BuildIdToken(query.AuthorizationCodeGrant.Id, request.ClientId, code.Scopes, query.Nonce.Value, query.UserId, query.SessionId, query.AuthorizationCodeGrant.AuthTime, cancellationToken: cancellationToken);

    await _identityContext.SaveChangesAsync(cancellationToken: cancellationToken);
    return new RedeemAuthorizationCodeGrantResponse(HttpStatusCode.OK)
    {
      AccessToken = accessToken,
      RefreshToken = refreshToken,
      IdToken = idToken,
      ExpiresIn = _identityConfiguration.AccessTokenExpiration
    };
  }
}
