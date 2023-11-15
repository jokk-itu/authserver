using System.Net;
using Application;
using Domain;
using Domain.Constants;
using Infrastructure.Builders.Token.Abstractions;
using Infrastructure.Builders.Token.GrantAccessToken;
using Infrastructure.Builders.Token.IdToken;
using Infrastructure.Builders.Token.RefreshToken;
using Infrastructure.Decoders.Token;
using Infrastructure.Decoders.Token.Abstractions;
using Infrastructure.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Requests.RedeemRefreshTokenGrant;
public class RedeemRefreshTokenGrantHandler : IRequestHandler<RedeemRefreshTokenGrantCommand, RedeemRefreshTokenGrantResponse>
{
  private readonly IdentityConfiguration _identityConfiguration;
  private readonly IStructuredTokenDecoder _tokenDecoder;
  private readonly ITokenBuilder<GrantAccessTokenArguments> _accessTokenBuilder;
  private readonly ITokenBuilder<RefreshTokenArguments> _refreshTokenBuilder;
  private readonly ITokenBuilder<IdTokenArguments> _idTokenBuilder;
  private readonly IdentityContext _identityContext;

  public RedeemRefreshTokenGrantHandler(
    IdentityConfiguration identityConfiguration,
    IStructuredTokenDecoder tokenDecoder,
    ITokenBuilder<GrantAccessTokenArguments> accessTokenBuilder,
    ITokenBuilder<RefreshTokenArguments> refreshTokenBuilder,
    ITokenBuilder<IdTokenArguments> idTokenBuilder,
    IdentityContext identityContext)
  {
    _identityConfiguration = identityConfiguration;
    _tokenDecoder = tokenDecoder;
    _accessTokenBuilder = accessTokenBuilder;
    _refreshTokenBuilder = refreshTokenBuilder;
    _idTokenBuilder = idTokenBuilder;
    _identityContext = identityContext;
  }

  public async Task<RedeemRefreshTokenGrantResponse> Handle(RedeemRefreshTokenGrantCommand request, CancellationToken cancellationToken)
  {
    string authorizationGrantId;
    if (request.RefreshToken.Split('.').Length == 3)
    {
      authorizationGrantId = await ValidateStructuredToken(request);
    }
    else
    {
      authorizationGrantId = await ValidateReferenceToken(request, cancellationToken);
    }

    string scope;
    if (string.IsNullOrWhiteSpace(request.Scope))
    {
      var consentedScopes = await GetConsentedScopes(authorizationGrantId, cancellationToken); 
      scope = string.Join(' ', consentedScopes.Select(x => x.Name));
    }
    else
    {
      scope = request.Scope;
    }

    var accessToken = await _accessTokenBuilder.BuildToken(new GrantAccessTokenArguments
    {
      Scope = scope,
      Resource = request.Resource,
      AuthorizationGrantId = authorizationGrantId
    });

    var refreshToken = await _refreshTokenBuilder.BuildToken(new RefreshTokenArguments
    {
      Scope = scope,
      AuthorizationGrantId = authorizationGrantId
    });

    var idToken = await _idTokenBuilder.BuildToken(new IdTokenArguments
    {
      AuthorizationGrantId = authorizationGrantId
    });
    await _identityContext.SaveChangesAsync(cancellationToken: cancellationToken);
    return new RedeemRefreshTokenGrantResponse(HttpStatusCode.OK)
    {
      AccessToken = accessToken,
      RefreshToken = refreshToken,
      IdToken = idToken,
      ExpiresIn = _identityConfiguration.AccessTokenExpiration
    };
  }

  private async Task<string> ValidateReferenceToken(RedeemRefreshTokenGrantCommand value, CancellationToken cancellationToken)
  {
    var authorizationGrantId = await _identityContext
      .Set<RefreshToken>()
      .Where(x => x.Reference == value.RefreshToken)
      .Select(x => x.AuthorizationGrant.Id)
      .SingleAsync(cancellationToken: cancellationToken);

    return authorizationGrantId;
  }

  private async Task<string> ValidateStructuredToken(RedeemRefreshTokenGrantCommand value)
  {
    var token = await _tokenDecoder.Decode(value.RefreshToken, new StructuredTokenDecoderArguments
      {
        ClientId = value.ClientId,
      });
    var authorizationGrantId = token.Claims.Single(x => x.Type == ClaimNameConstants.GrantId).Value;
    return authorizationGrantId;
  }

  private async Task<IEnumerable<Scope>> GetConsentedScopes(string authorizationGrantId, CancellationToken cancellationToken)
  {
    var consentedScopes = await _identityContext
      .Set<AuthorizationCodeGrant>()
      .Where(x => x.Id == authorizationGrantId)
      .Select(x => new
      {
        UserId = x.Session.User.Id,
        ClientId = x.Client.Id
      })
      .Join(
        _identityContext.Set<ConsentGrant>(),
        outer => outer,
        inner => new
        {
          UserId = inner.User.Id,
          ClientId = inner.Client.Id
        },
        (inner, outer) => outer)
      .Select(x => x.ConsentedScopes)
      .SingleAsync(cancellationToken: cancellationToken);

    return consentedScopes;
  }
}