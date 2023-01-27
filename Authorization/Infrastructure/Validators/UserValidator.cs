using Application;
using Application.Validation;
using Infrastructure.Services.Abstract;

namespace Infrastructure.Validators;
public class UserValidator : IBaseValidator<UserToValidate>
{
  private readonly IUserService _userService;

  public UserValidator(IUserService userService)
  {
    _userService = userService;
  }

  public async Task<BaseValidationResult> ValidateAsync(UserToValidate value, CancellationToken cancellationToken = default)
  {
    if (string.IsNullOrWhiteSpace(value.Username) ||
        string.IsNullOrWhiteSpace(value.Password))
    {
      return new BaseValidationResult(ErrorCode.InvalidRequest, "user is invalid");
    }

    if (!await _userService.IsValid(value.Username, value.Password, cancellationToken))
    {
      return new BaseValidationResult(ErrorCode.InvalidRequest, "user is invalid");
    }

    return new BaseValidationResult();
  }
}

public record UserToValidate(string Username, string Password);