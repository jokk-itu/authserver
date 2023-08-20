using Application.Validation;

namespace Infrastructure.Services.Abstract;
public interface IScopeService
{
  Task<BaseValidationResult> ValidateScope(string scope, CancellationToken cancellationToken);
}
