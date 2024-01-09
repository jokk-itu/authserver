using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Application;
using Application.Validation;
using Domain.Constants;
using Microsoft.IdentityModel.Tokens;

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

  public static BaseValidationResult ValidateCodeVerifier(string codeVerifier, string codeChallenge)
  {
    var isCodeVerifierInvalid = string.IsNullOrWhiteSpace(codeVerifier) ||
                                !Regex.IsMatch(codeVerifier,
                                  "^[0-9a-zA-Z-_~.]{43,128}$",
                                  RegexOptions.Compiled,
                                  TimeSpan.FromMilliseconds(100));

    if (isCodeVerifierInvalid)
    {
      return new BaseValidationResult(ErrorCode.InvalidRequest, "code_verifier is invalid");
    }

    using var sha256 = SHA256.Create();
    var bytes = Encoding.UTF8.GetBytes(codeVerifier);
    var hashed = sha256.ComputeHash(bytes);
    var encoded = Base64UrlEncoder.Encode(hashed);
    if (encoded != codeChallenge)
    {
      return new BaseValidationResult(ErrorCode.InvalidRequest, "code_verifier is invalid");
    }

    return new BaseValidationResult();
  }
}