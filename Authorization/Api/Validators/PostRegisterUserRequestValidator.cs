using Contracts.RegisterUser;
using FluentValidation;

namespace Api.Validators;

public class PostRegisterUserRequestValidator : AbstractValidator<PostRegisterUserRequest>
{
  public PostRegisterUserRequestValidator()
  {
    RuleFor(x => x.Username).NotEmpty();
    RuleFor(x => x.Password).NotEmpty();
    RuleFor(x => x.Email).NotEmpty();
  }
}
