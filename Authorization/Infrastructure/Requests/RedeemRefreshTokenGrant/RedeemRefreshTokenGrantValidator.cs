using System.Net;
using Application;
using Application.Validation;
using Domain;
using Domain.Constants;
using Domain.Enums;
using Infrastructure.Decoders.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Requests.RedeemRefreshTokenGrant;
public class RedeemRefreshTokenGrantValidator : IValidator<RedeemRefreshTokenGrantCommand>
{
  private readonly IdentityContext _identityContext;
  private readonly ITokenDecoder _tokenDecoder;

  public RedeemRefreshTokenGrantValidator(IdentityContext identityContext, ITokenDecoder tokenDecoder)
  {
    _identityContext = identityContext;
    _tokenDecoder = tokenDecoder;
  }

  public async Task<ValidationResult> ValidateAsync(RedeemRefreshTokenGrantCommand value, CancellationToken cancellationToken = default)
  {
    var refreshToken = _tokenDecoder.DecodeSignedToken(value.RefreshToken);
    if (refreshToken is null)
    {
      return new ValidationResult(ErrorCode.InvalidRequest, "refresh_token is invalid", HttpStatusCode.BadRequest);
    }

    if (value.GrantType != GrantTypeConstants.RefreshToken)
    {
      return new ValidationResult(ErrorCode.InvalidGrant, "grant_type must be refresh_token", HttpStatusCode.BadRequest);
    }
    var clientId = refreshToken.Claims.Single(x => x.Type == ClaimNameConstants.ClientId).Value;
    var sessionId = refreshToken.Claims.Single(x => x.Type == ClaimNameConstants.Sid).Value;
    var authorizationGrantId = refreshToken.Claims.Single(x => x.Type == ClaimNameConstants.GrantId).Value;

    if (clientId != value.ClientId)
    {
      return new ValidationResult(ErrorCode.AccessDenied, "client_id does not match client_id in refresh_token", HttpStatusCode.BadRequest);
    }

    var query = await _identityContext
      .Set<AuthorizationCodeGrant>()
      .Where(x => x.Id == authorizationGrantId)
      .Where(AuthorizationCodeGrant.IsMaxAgeValid)
      .Select(x => new
      {
        IsClientIdValid = x.Client.Id == value.ClientId,
        IsClientSecretValid = x.Client.Secret == value.ClientSecret,
        HasClientSecret = x.Client.TokenEndpointAuthMethod == TokenEndpointAuthMethod.None,
        IsClientAuthorized = x.Client.GrantTypes.Any(y => y.Name == GrantTypeConstants.RefreshToken),
        IsSessionValid = !x.Session.IsRevoked && x.Session.Id == sessionId
      })
      .SingleOrDefaultAsync(cancellationToken: cancellationToken);

    if (query is null)
    {
      return new ValidationResult(ErrorCode.LoginRequired, "authorization grant is invalid", HttpStatusCode.BadRequest);
    }

    if (!query.IsClientIdValid)
    {
      return new ValidationResult(ErrorCode.InvalidClient, "client_id is invalid", HttpStatusCode.BadRequest);
    }

    if (!query.IsClientNative && !query.IsClientSecretValid)
    {
      return new ValidationResult(ErrorCode.InvalidClient, "client_secret is invalid", HttpStatusCode.BadRequest);
    }

    if (!query.IsClientAuthorized)
    {
      return new ValidationResult(ErrorCode.UnauthorizedClient, "client is unauthorized", HttpStatusCode.BadRequest);
    }

    if (!query.IsSessionValid)
    {
      return new ValidationResult(ErrorCode.LoginRequired, "session is invalid", HttpStatusCode.BadRequest);
    }

    return new ValidationResult(HttpStatusCode.OK);
  }
}
