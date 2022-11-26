using System.Net;
using Application.Validation;
using Infrastructure.Validators;

namespace Infrastructure.Requests.GetLoginToken;
public class GetLoginTokenValidator : IValidator<GetLoginTokenQuery>
{
  private readonly IBaseValidator<UserToValidate> _userValidator;

  public GetLoginTokenValidator(
    IBaseValidator<UserToValidate> userValidator)
  {
    _userValidator = userValidator;
  }

  public async Task<ValidationResult> ValidateAsync(GetLoginTokenQuery value, CancellationToken cancellationToken = default)
  {
    var userValidation = await _userValidator.ValidateAsync(new UserToValidate(value.Username, value.Password), cancellationToken: cancellationToken);
    if (userValidation.IsError())
      return new ValidationResult(userValidation.ErrorCode, userValidation.ErrorDescription, HttpStatusCode.BadRequest);

    return new ValidationResult(HttpStatusCode.OK);
  }
}