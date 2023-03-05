using System.Net;
using Application;
using Application.Validation;
using Domain;
using Domain.Constants;
using Infrastructure.Builders.Abstractions;
using Infrastructure.Decoders.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Requests.RedeemRefreshTokenGrant;
public class RedeemRefreshTokenGrantHandler : IRequestHandler<RedeemRefreshTokenGrantCommand, RedeemRefreshTokenGrantResponse>
{
  private readonly IValidator<RedeemRefreshTokenGrantCommand> _validator;
  private readonly ITokenDecoder _tokenDecoder;
  private readonly ITokenBuilder _tokenBuilder;
  private readonly IdentityConfiguration _identityConfiguration;
  private readonly IdentityContext _identityContext;

  public RedeemRefreshTokenGrantHandler(
    IValidator<RedeemRefreshTokenGrantCommand> validator,
    ITokenDecoder tokenDecoder,
    ITokenBuilder tokenBuilder,
    IdentityConfiguration identityConfiguration,
    IdentityContext identityContext)
  {
    _validator = validator;
    _tokenDecoder = tokenDecoder;
    _tokenBuilder = tokenBuilder;
    _identityConfiguration = identityConfiguration;
    _identityContext = identityContext;
  }

  public async Task<RedeemRefreshTokenGrantResponse> Handle(RedeemRefreshTokenGrantCommand request, CancellationToken cancellationToken)
  {
    var validationResult = await _validator.ValidateAsync(request, cancellationToken);
    if (validationResult.IsError())
    {
      return new RedeemRefreshTokenGrantResponse(validationResult.ErrorCode, validationResult.ErrorDescription, validationResult.StatusCode);
    }

    var token = _tokenDecoder.DecodeSignedToken(request.RefreshToken);
    if (token is null)
    {
      throw new SecurityTokenException("Decode must not fail after successful validation");
    }

    var scopes = token.Claims.Single(x => x.Type == ClaimNameConstants.Scope).Value.Split(' ');
    var userId = token.Claims.Single(x => x.Type == ClaimNameConstants.Sub).Value;
    var sessionId = token.Claims.Single(x => x.Type == ClaimNameConstants.Sid).Value;
    var authorizationGrantId = token.Claims.Single(x => x.Type == ClaimNameConstants.GrantId).Value;
    var query = await _identityContext
      .Set<AuthorizationCodeGrant>()
      .Where(x => x.Id == authorizationGrantId)
      .Select(x => new
      {
        AuthorizationCodeGrant = x,
        Nonce = x.Nonces.OrderByDescending(y => y.Id).First()
      })
      .FirstAsync(cancellationToken: cancellationToken);

    var refreshToken = await _tokenBuilder.BuildRefreshTokenAsync(authorizationGrantId, request.ClientId, scopes, userId, sessionId, cancellationToken: cancellationToken);
    var accessToken = await _tokenBuilder.BuildAccessTokenAsync(request.ClientId, scopes, userId, sessionId, cancellationToken: cancellationToken);
    var idToken = await _tokenBuilder.BuildIdTokenAsync(authorizationGrantId, request.ClientId, scopes, query.Nonce.Value, userId,
      sessionId, query.AuthorizationCodeGrant.AuthTime, cancellationToken: cancellationToken);

    return new RedeemRefreshTokenGrantResponse(HttpStatusCode.OK)
    {
      AccessToken = accessToken,
      RefreshToken = refreshToken,
      IdToken = idToken,
      ExpiresIn = _identityConfiguration.RefreshTokenExpiration
    };
  }
}