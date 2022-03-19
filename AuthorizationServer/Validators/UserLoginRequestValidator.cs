using AuthorizationServer.Constants;
using AuthorizationServer.Requests;
using FluentValidation;

namespace AuthorizationServer.Validators;

public class UserLoginRequestValidator : AbstractValidator<UserLoginRequest>
{
    public UserLoginRequestValidator()
    {
        RuleFor(x => x.Username).NotEmpty().WithErrorCode(UserLoginErrorCode.InvalidRequest)
            .WithMessage(UserLoginErrorDescription.Username);
        RuleFor(x => x.Password).NotEmpty().WithErrorCode(UserLoginErrorCode.InvalidRequest)
            .WithMessage(UserLoginErrorDescription.Password);
    }
}