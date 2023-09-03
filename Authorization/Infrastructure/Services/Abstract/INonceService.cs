using Application.Validation;

namespace Infrastructure.Services.Abstract;
public interface INonceService
{
  Task<BaseValidationResult> ValidateNonce(string nonce, CancellationToken cancellationToken);
}
