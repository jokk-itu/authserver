using Application;
using Application.Validation;
using Domain.Constants;

namespace Infrastructure.BaseValidators;
public class ScopeValidator : BaseValidator<ICollection<string>>
{
  public ScopeValidator(bool isRequired) : base(isRequired)
  {
  }

  public override Task<BaseValidationResult> IsValidAsync(ICollection<string> value)
  {
    if (IsRequired && value.All(x => x != ScopeConstants.OpenId))
      return GetInvalidResult(ErrorCode.InvalidScope, $"scope must at least contain {ScopeConstants.OpenId}");

    return GetValidResult();
  }
}