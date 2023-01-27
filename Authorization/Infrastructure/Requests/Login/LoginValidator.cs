using System.Net;
using Application.Validation;
using Infrastructure.Validators;

namespace Infrastructure.Requests.Login;
public class LoginValidator : IValidator<LoginQuery>
{
  private readonly IBaseValidator<UserToValidate> _userValidator;

  public LoginValidator(
    IBaseValidator<UserToValidate> userValidator)
  {
    _userValidator = userValidator;
  }

  public async Task<ValidationResult> ValidateAsync(LoginQuery value, CancellationToken cancellationToken = default)
  {
    var userValidation = await _userValidator.ValidateAsync(new UserToValidate(value.Username, value.Password), cancellationToken: cancellationToken);
    if (userValidation.IsError())
    {
      return new ValidationResult(userValidation.ErrorCode, userValidation.ErrorDescription, HttpStatusCode.BadRequest);
    }

    return new ValidationResult(HttpStatusCode.OK);
  }
}