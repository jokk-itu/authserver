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
  private readonly ResourceManager _resourceManager;

  public RedeemAuthorizationCodeGrantHandler(
    IdentityContext identityContext,
    IdentityConfiguration identityConfiguration,
    ICodeDecoder codeDecoder,
    ITokenBuilder<GrantAccessTokenArguments> accessTokenBuilder,
    ITokenBuilder<RefreshTokenArguments> refreshTokenBuilder,
    ITokenBuilder<IdTokenArguments> idTokenBuilder,
    ResourceManager resourceManager)
  {
    _identityContext = identityContext;
    _identityConfiguration = identityConfiguration;
    _codeDecoder = codeDecoder;
    _accessTokenBuilder = accessTokenBuilder;
    _refreshTokenBuilder = refreshTokenBuilder;
    _idTokenBuilder = idTokenBuilder;
    _resourceManager = resourceManager;
  }

  public async Task<RedeemAuthorizationCodeGrantResponse> Handle(RedeemAuthorizationCodeGrantCommand request, CancellationToken cancellationToken)
  {
    var code = _codeDecoder.DecodeAuthorizationCode(request.Code);
    var resources = await _resourceManager.ReadResourcesAsync(request.Scope.Split(' '), cancellationToken: cancellationToken);
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
        Scope = request.Scope
      });
    }

    var accessToken = await _accessTokenBuilder.BuildToken(new GrantAccessTokenArguments
    {
      AuthorizationGrantId = query.AuthorizationCodeGrant.Id,
      Scope = request.Scope,
      ResourceNames = resources.Select(x => x.Name)
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
      ExpiresIn = _identityConfiguration.AccessTokenExpiration
    };
  }
}