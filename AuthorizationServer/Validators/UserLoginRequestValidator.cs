using AuthorizationServer.Requests;
using FluentValidation;

namespace AuthorizationServer.Validators;

public class UserLoginRequestValidator : AbstractValidator<UserLoginRequest>
{
    public UserLoginRequestValidator()
    {
        
    }    
}