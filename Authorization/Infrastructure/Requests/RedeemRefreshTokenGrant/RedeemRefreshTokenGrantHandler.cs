using System.IdentityModel.Tokens.Jwt;
using System.Net;
using Application.Validation;
using Domain.Constants;
using Infrastructure.Builders.Abstractions;
using Infrastructure.Decoders.Abstractions;
using MediatR;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Requests.CreateRefreshTokenGrant;
public class RedeemRefreshTokenGrantHandler : IRequestHandler<RedeemRefreshTokenGrantCommand, RedeemRefreshTokenGrantResponse>
{
  private readonly IValidator<RedeemRefreshTokenGrantCommand> _validator;
  private readonly ITokenDecoder _tokenDecoder;
  private readonly ITokenBuilder _tokenBuilder;
  private readonly IdentityConfiguration _identityConfiguration;

  public RedeemRefreshTokenGrantHandler(
    IValidator<RedeemRefreshTokenGrantCommand> validator,
    ITokenDecoder tokenDecoder,
    ITokenBuilder tokenBuilder,
    IdentityConfiguration identityConfiguration)
  {
    _validator = validator;
    _tokenDecoder = tokenDecoder;
    _tokenBuilder = tokenBuilder;
    _identityConfiguration = identityConfiguration;
  }

  public async Task<RedeemRefreshTokenGrantResponse> Handle(RedeemRefreshTokenGrantCommand request, CancellationToken cancellationToken)
  {
    var validationResult = await _validator.ValidateAsync(request, cancellationToken);
    if (validationResult.IsError())
      return new RedeemRefreshTokenGrantResponse(validationResult.ErrorCode, validationResult.ErrorDescription, validationResult.StatusCode);

    var token = _tokenDecoder.DecodeToken(request.RefreshToken);
    if (token is null)
      throw new SecurityTokenException("Decode must not fail after successful validation");

    var scopes = token.Claims.Single(x => x.Type == ClaimNameConstants.Scope).Value.Split(' ');
    var userId = token.Claims.Single(x => x.Type == JwtRegisteredClaimNames.Sub).Value;

    var refreshToken = await _tokenBuilder.BuildRefreshTokenAsync(request.ClientId, scopes, userId, cancellationToken: cancellationToken);
    var accessToken = await _tokenBuilder.BuildAccessTokenAsync(request.ClientId, scopes, userId, cancellationToken: cancellationToken);
    return new RedeemRefreshTokenGrantResponse(HttpStatusCode.OK)
    {
      AccessToken = accessToken,
      RefreshToken = refreshToken,
      ExpiresIn = _identityConfiguration.RefreshTokenExpiration
    };
  }
}
