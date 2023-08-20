using Application;
using Application.Validation;
using Domain;
using Infrastructure.Services.Abstract;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;
public class NonceService : INonceService
{
  private readonly IdentityContext _identityContext;

  public NonceService(
    IdentityContext identityContext)
  {
    _identityContext = identityContext;
  }

  public async Task<BaseValidationResult> ValidateNonce(string nonce, CancellationToken cancellationToken)
  {
    if (string.IsNullOrWhiteSpace(nonce))
    {
      return new BaseValidationResult(ErrorCode.InvalidRequest, "nonce is invalid");
    }

    var isNonceNotUnique = await _identityContext
      .Set<Nonce>()
      .Where(x => x.Value == nonce)
      .AnyAsync(cancellationToken: cancellationToken);

    if (isNonceNotUnique)
    {
      return new BaseValidationResult(
        ErrorCode.InvalidRequest,
        "nonce is not unique");
    }

    return new BaseValidationResult();
  }
}