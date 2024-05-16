using System.Net;
using Application;
using Application.Validation;
using Infrastructure.Services.Abstract;
using Infrastructure.Validators;

namespace Infrastructure.Requests.Login;
public class LoginValidator : IValidator<LoginQuery>
{
  private readonly IUserService _userService;

  public LoginValidator(
    IUserService userService)
  {
    _userService = userService;
  }

  public async Task<ValidationResult> ValidateAsync(LoginQuery value, CancellationToken cancellationToken = default)
  {
    if (string.IsNullOrWhiteSpace(value.Username) ||
        string.IsNullOrWhiteSpace(value.Password))
    {
      return new ValidationResult(
        ErrorCode.InvalidRequest,
        "user is invalid",
        HttpStatusCode.BadRequest);
    }

    if (!await _userService.IsValid(value.Username, value.Password, cancellationToken))
    {
      return new ValidationResult(
        ErrorCode.InvalidRequest,
        "user is invalid",
        HttpStatusCode.BadRequest);
    }

    return new ValidationResult(HttpStatusCode.OK);
  }
}