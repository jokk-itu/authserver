using System.Net;
using Application;
using Application.Validation;
using Domain;
using Domain.Constants;
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
    var sessionClaim = refreshToken.Claims.Single(x => x.Type == ClaimNameConstants.Sid);
    var sessionId = long.Parse(sessionClaim.Value);

    if (clientId != value.ClientId)
    {
      return new ValidationResult(ErrorCode.InvalidRequest, "refresh_token is invalid", HttpStatusCode.BadRequest);
    }

    var client = await _identityContext
      .Set<Client>()
      .Where(c => c.Id == value.ClientId)
      .Where(c => c.Secret == value.ClientSecret)
      .Include(c => c.GrantTypes)
      .SingleOrDefaultAsync(cancellationToken: cancellationToken);

    if (client is null)
    {
      return new ValidationResult(ErrorCode.InvalidClient, "client is invalid", HttpStatusCode.BadRequest);
    }

    if (client.GrantTypes.All(x => x.Name != GrantTypeConstants.RefreshToken))
    {
      return new ValidationResult(ErrorCode.UnauthorizedClient, "client is unauthorized", HttpStatusCode.BadRequest);
    }

    var session = await _identityContext
      .Set<Session>()
      .Where(s => s.Id == sessionId)
      .Where(Session.IsValid)
      .Where(s => s.AuthorizationCodeGrants.Any(acg => acg.Client.Id == value.ClientId))
      .SingleOrDefaultAsync(cancellationToken: cancellationToken);

    if (session is null)
    {
      return new ValidationResult(ErrorCode.LoginRequired, "session is invalid", HttpStatusCode.Unauthorized);
    }

    return new ValidationResult(HttpStatusCode.OK);
  }
}
