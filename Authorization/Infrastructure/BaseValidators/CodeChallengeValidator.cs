using System.Text.RegularExpressions;
using Application;
using Application.Validation;

namespace Infrastructure.BaseValidators;
public class CodeChallengeValidator : BaseValidator<string>
{
  public CodeChallengeValidator(bool isRequired) : base(isRequired)
  {
    
  }

  public override Task<BaseValidationResult> IsValidAsync(string value)
  {
    return IsRequired switch
    {
      true when !string.IsNullOrWhiteSpace(value) => GetInvalidValidationResult(),
      true when !Regex.IsMatch(value, "^[0-9a-zA-Z-_~.]{43,128}$") => GetInvalidValidationResult(),
      _ => GetValidResult()
    };
  }

  private Task<BaseValidationResult> GetInvalidValidationResult()
  {
    return GetInvalidResult(ErrorCode.InvalidRequest, "code_challenge is invalid");
  }
}
