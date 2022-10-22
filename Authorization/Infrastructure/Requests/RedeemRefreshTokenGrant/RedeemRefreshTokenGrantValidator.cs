using System.Net;
using Application;
using Application.Validation;
using Domain;
using Domain.Constants;
using Infrastructure.Decoders.Abstractions;
using Infrastructure.Requests.CreateRefreshTokenGrant;
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
    if (await IsClientInvalidAsync(value))
      return new ValidationResult(ErrorCode.InvalidClient, "client is invalid", HttpStatusCode.BadRequest);

    if (IsRefreshTokenInvalid(value))
      return new ValidationResult(ErrorCode.InvalidRequest, "refresh_token is invalid", HttpStatusCode.BadRequest);

    if(await IsSessionInvalidAsync(value))
      return new ValidationResult(ErrorCode.AccessDenied, "session is invalid", HttpStatusCode.Unauthorized);

    return new ValidationResult(HttpStatusCode.OK);
  }

  private async Task<bool> IsClientInvalidAsync(RedeemRefreshTokenGrantCommand command)
  {
    var client = await _identityContext
      .Set<Client>()
      .SingleOrDefaultAsync(x => x.Id == command.ClientId 
                                 && x.Secret == command.ClientSecret);
    return client is null;
  }

  private bool IsRefreshTokenInvalid(RedeemRefreshTokenGrantCommand command)
  {
    if (string.IsNullOrWhiteSpace(command.RefreshToken))
      return true;

    var refreshToken = _tokenDecoder.DecodeToken(command.RefreshToken);
    return refreshToken is null;
  }

  private async Task<bool> IsSessionInvalidAsync(RedeemRefreshTokenGrantCommand command)
  {
    var refreshToken = _tokenDecoder.DecodeToken(command.RefreshToken);
    if(refreshToken is null)
      return true;

    var sessionClaim = refreshToken.Claims.Single(x => x.Type == ClaimNameConstants.Sid);
    var sessionId = long.Parse(sessionClaim.Value);
    var session = await _identityContext
      .Set<Session>()
      .SingleOrDefaultAsync(x => x.Id == sessionId);

    return session is null || session.IsInvalid();
  }
}
