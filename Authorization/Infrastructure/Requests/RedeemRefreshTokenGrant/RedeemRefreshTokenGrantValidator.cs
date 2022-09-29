using System.Net;
using Application;
using Application.Validation;
using Domain;
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

    if (IsInvalidRefreshToken(value))
      return new ValidationResult(ErrorCode.InvalidRequest, "refresh_token is invalid", HttpStatusCode.BadRequest);

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

  private bool IsInvalidRefreshToken(RedeemRefreshTokenGrantCommand command)
  {
    if (string.IsNullOrWhiteSpace(command.RefreshToken))
      return true;

    var refreshToken = _tokenDecoder.DecodeToken(command.RefreshToken);
    return refreshToken is null;
  }
}
