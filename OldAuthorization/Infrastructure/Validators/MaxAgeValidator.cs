using Application;
using Application.Validation;

namespace Infrastructure.Validators;
public static class MaxAgeValidator
{
  public static BaseValidationResult ValidateMaxAge(string maxAge)
  {
    if (!string.IsNullOrWhiteSpace(maxAge)
        && !(long.TryParse(maxAge, out var parsedMaxAge)
             && parsedMaxAge > -1))
    {
      return new BaseValidationResult(ErrorCode.InvalidRequest,
        "max_age is invalid");
    }

    return new BaseValidationResult();
  }
}
