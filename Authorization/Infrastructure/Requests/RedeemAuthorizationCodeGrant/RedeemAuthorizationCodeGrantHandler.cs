using System.Net;
using Application;
using Domain;
using Domain.Constants;
using Infrastructure.Builders.Token.Abstractions;
using Infrastructure.Builders.Token.GrantAccessToken;
using Infrastructure.Builders.Token.IdToken;
using Infrastructure.Builders.Token.RefreshToken;
using Infrastructure.Decoders.Abstractions;
using Infrastructure.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Requests.RedeemAuthorizationCodeGrant;
public class RedeemAuthorizationCodeGrantHandler : IRequestHandler<RedeemAuthorizationCodeGrantCommand, RedeemAuthorizationCodeGrantResponse>
{
  private readonly IdentityContext _identityContext;
  private readonly IdentityConfiguration _identityConfiguration;
  private readonly ICodeDecoder _codeDecoder;
  private readonly ITokenBuilder<GrantAccessTokenArguments> _accessTokenBuilder;
  private readonly ITokenBuilder<RefreshTokenArguments> _refreshTokenBuilder;
  private readonly ITokenBuilder<IdTokenArguments> _idTokenBuilder;

  public RedeemAuthorizationCodeGrantHandler(
    IdentityContext identityContext,
    IdentityConfiguration identityConfiguration,
    ICodeDecoder codeDecoder,
    ITokenBuilder<GrantAccessTokenArguments> accessTokenBuilder,
    ITokenBuilder<RefreshTokenArguments> refreshTokenBuilder,
    ITokenBuilder<IdTokenArguments> idTokenBuilder)
  {
    _identityContext = identityContext;
    _identityConfiguration = identityConfiguration;
    _codeDecoder = codeDecoder;
    _accessTokenBuilder = accessTokenBuilder;
    _refreshTokenBuilder = refreshTokenBuilder;
    _idTokenBuilder = idTokenBuilder;
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
        Nonce = x.Nonces.Single(y => y.Id == code.NonceId),
        IsAuthorizedToRefresh = x.Client.GrantTypes.Any(y => y.Name == GrantTypeConstants.RefreshToken)
      })
      .SingleAsync(cancellationToken: cancellationToken);

    query.AuthorizationCode.IsRedeemed = true;
    query.AuthorizationCode.RedeemedAt = DateTime.UtcNow;

    string? refreshToken = null;
    if (query.IsAuthorizedToRefresh)
    {
      refreshToken = await _refreshTokenBuilder.BuildToken(new RefreshTokenArguments
      {
        AuthorizationGrantId = query.AuthorizationCodeGrant.Id,
        Scope = string.Join(' ', code.Scopes)
      });
    }

    var accessToken = await _accessTokenBuilder.BuildToken(new GrantAccessTokenArguments
    {
      AuthorizationGrantId = query.AuthorizationCodeGrant.Id,
      Scope = string.Join(' ', code.Scopes),
      Resource = request.Resource
    });

    var idToken = await _idTokenBuilder.BuildToken(new IdTokenArguments
    {
      AuthorizationGrantId = query.AuthorizationCodeGrant.Id
    });

    await _identityContext.SaveChangesAsync(cancellationToken: cancellationToken);
    return new RedeemAuthorizationCodeGrantResponse(HttpStatusCode.OK)
    {
      AccessToken = accessToken,
      RefreshToken = refreshToken,
      IdToken = idToken,
      ExpiresIn = _identityConfiguration.AccessTokenExpiration,
      Scope = string.Join(' ', code.Scopes)
    };
  }
}