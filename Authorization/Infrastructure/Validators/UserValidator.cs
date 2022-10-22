using Application;
using Application.Validation;
using Domain;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Validators;
public class UserValidator : BaseValidator<UserToValidate>
{
  private readonly UserManager<User> _userManager;

  public UserValidator(UserManager<User> userManager)
  {
    _userManager = userManager;
  }

  public override async Task<BaseValidationResult> ValidateAsync(UserToValidate value, CancellationToken cancellationToken = default)
  {
    if (string.IsNullOrWhiteSpace(value.Username) ||
        string.IsNullOrWhiteSpace(value.Password))
      return new BaseValidationResult(ErrorCode.InvalidRequest, "user is invalid");

    var user = await _userManager.FindByNameAsync(value.Username);
    if (user is null)
      return new BaseValidationResult(ErrorCode.InvalidRequest, "user is invalid");

    if(!await _userManager.CheckPasswordAsync(user, value.Password))
      return new BaseValidationResult(ErrorCode.InvalidRequest, "user is invalid");

    return Ok();
  }
}

public record UserToValidate(string Username, string Password);