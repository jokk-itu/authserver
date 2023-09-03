using System.Text.RegularExpressions;
using Application;
using Application.Validation;
using Domain.Constants;

namespace Infrastructure.Validators;
public static class PkceValidator
{
  public static BaseValidationResult ValidateCodeChallengeMethod(string codeChallengeMethod)
  {
    return codeChallengeMethod == CodeChallengeMethodConstants.S256
      ? new BaseValidationResult()
      : new BaseValidationResult(
        ErrorCode.InvalidRequest,
        "code_challenge_method is invalid");
  }

  public static BaseValidationResult ValidateCodeChallenge(string codeChallenge)
  {
    return !string.IsNullOrWhiteSpace(codeChallenge) && Regex.IsMatch(
      codeChallenge,
      "^[0-9a-zA-Z-_~.]{43,128}$",
      RegexOptions.Compiled,
      TimeSpan.FromMilliseconds(100))
      ? new BaseValidationResult()
      : new BaseValidationResult(
        ErrorCode.InvalidRequest,
        "code_challenge is invalid");
  }
}