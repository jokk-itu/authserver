using System.Net;
using Application;
using Domain;
using Domain.Constants;
using Infrastructure.Builders.Abstractions;
using Infrastructure.Builders.Token.Abstractions;
using Infrastructure.Builders.Token.GrantAccessToken;
using Infrastructure.Builders.Token.IdToken;
using Infrastructure.Builders.Token.RefreshToken;
using Infrastructure.Decoders.Abstractions;
using Infrastructure.Decoders.Token;
using Infrastructure.Decoders.Token.Abstractions;
using Infrastructure.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Requests.RedeemRefreshTokenGrant;
public class RedeemRefreshTokenGrantHandler : IRequestHandler<RedeemRefreshTokenGrantCommand, RedeemRefreshTokenGrantResponse>
{
  private readonly IdentityConfiguration _identityConfiguration;
  private readonly IStructuredTokenDecoder _tokenDecoder;
  private readonly ITokenBuilder<GrantAccessTokenArguments> _accessTokenBuilder;
  private readonly ITokenBuilder<RefreshTokenArguments> _refreshTokenBuilder;
  private readonly ITokenBuilder<IdTokenArguments> _idTokenBuilder;
  private readonly ResourceManager _resourceManager;
  private readonly IdentityContext _identityContext;

  public RedeemRefreshTokenGrantHandler(
    IdentityConfiguration identityConfiguration,
    IStructuredTokenDecoder tokenDecoder,
    ITokenBuilder<GrantAccessTokenArguments> accessTokenBuilder,
    ITokenBuilder<RefreshTokenArguments> refreshTokenBuilder,
    ITokenBuilder<IdTokenArguments> idTokenBuilder,
    ResourceManager resourceManager,
    IdentityContext identityContext)
  {
    _identityConfiguration = identityConfiguration;
    _tokenDecoder = tokenDecoder;
    _accessTokenBuilder = accessTokenBuilder;
    _refreshTokenBuilder = refreshTokenBuilder;
    _idTokenBuilder = idTokenBuilder;
    _resourceManager = resourceManager;
    _identityContext = identityContext;
  }

  public async Task<RedeemRefreshTokenGrantResponse> Handle(RedeemRefreshTokenGrantCommand request, CancellationToken cancellationToken)
  {
    var token = await _tokenDecoder.Decode(request.RefreshToken, new StructuredTokenDecoderArguments
    {
      ClientId = request.ClientId
    });

    var authorizationGrantId = token.Claims.Single(x => x.Type == ClaimNameConstants.GrantId).Value;
    // TODO get the consented scopes from the ConsentGrant if the request.Scope is null
    var scope = request.Scope ?? "";
    var resources = await _resourceManager.ReadResourcesAsync(scope.Split(' '), cancellationToken: cancellationToken);

    var accessToken = await _accessTokenBuilder.BuildToken(new GrantAccessTokenArguments
    {
      Scope = scope,
      ResourceNames = resources.Select(x => x.Name),
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

    return new RedeemRefreshTokenGrantResponse(HttpStatusCode.OK)
    {
      AccessToken = accessToken,
      RefreshToken = refreshToken,
      IdToken = idToken,
      ExpiresIn = _identityConfiguration.RefreshTokenExpiration
    };
  }
}