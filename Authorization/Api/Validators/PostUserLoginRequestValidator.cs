using AuthorizationServer.Constants;
using Contracts.PostUserLogin;
using FluentValidation;

namespace AuthorizationServer.Validators;

public class PostUserLoginRequestValidator : AbstractValidator<PostUserLoginRequest>
{
  public PostUserLoginRequestValidator()
  {
    RuleFor(x => x.Username).NotEmpty().WithErrorCode(UserLoginErrorCode.InvalidRequest)
        .WithMessage(UserLoginErrorDescription.Username);
    RuleFor(x => x.Password).NotEmpty().WithErrorCode(UserLoginErrorCode.InvalidRequest)
        .WithMessage(UserLoginErrorDescription.Password);
  }
}