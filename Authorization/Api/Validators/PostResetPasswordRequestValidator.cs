using Contracts.ResetPassword;
using FluentValidation;

namespace Api.Validators;

public class PostResetPasswordRequestValidator : AbstractValidator<PostResetPasswordRequest>
{
  public PostResetPasswordRequestValidator()
  {
    RuleFor(x => x.Username).NotEmpty();
    RuleFor(x => x.Password).NotEmpty();
  }
}
