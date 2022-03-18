using AuthorizationServer.Constants;
using AuthorizationServer.Requests;
using FluentValidation;
using FluentValidation.Results;

namespace AuthorizationServer.Validators;

public class AuthorizeRequestValidator : AbstractValidator<AuthorizeCodeRequest>
{
    public AuthorizeRequestValidator()
    {
        RuleFor(x => x.UserInformation).NotNull().WithErrorCode(AuthorizeCodeErrorCode.InvalidRequest).WithMessage(AuthorizeCodeErrorDescription.UserInformation);
        RuleFor(x => x.ResponseType)
            .Matches(
                $"^({AuthorizeCodeResponseType.Code} {AuthorizeCodeResponseType.IdToken})|({AuthorizeCodeResponseType.Code})|({AuthorizeCodeResponseType.IdToken} {AuthorizeCodeResponseType.Code})$")
            .WithErrorCode(AuthorizeCodeErrorCode.UnsupportedResponseType).WithMessage(AuthorizeCodeErrorDescription.ResponseType);
        RuleFor(x => x.ClientId).NotEmpty().WithErrorCode(AuthorizeCodeErrorCode.InvalidRequest).WithMessage(AuthorizeCodeErrorDescription.ClientId);
        RuleFor(x => x.RedirectUri).Custom((rawUri, context) =>
        {
            if (!Uri.IsWellFormedUriString(rawUri, UriKind.Absolute))
                context.AddFailure(new ValidationFailure(nameof(context.PropertyName), AuthorizeCodeErrorDescription.RedirectUri)
                {
                    ErrorCode = AuthorizeCodeErrorCode.InvalidRequest
                });
        });
        RuleFor(x => x.Scope).NotEmpty().WithErrorCode(AuthorizeCodeErrorCode.InvalidRequest).WithMessage(AuthorizeCodeErrorDescription.Scope);
        RuleFor(x => x.State).NotEmpty().WithErrorCode(AuthorizeCodeErrorCode.InvalidRequest).WithMessage(AuthorizeCodeErrorDescription.State);
        RuleFor(x => x.CodeChallenge).Matches("^[a-zA-Z0-9-_~.]{43,128}$")
            .WithErrorCode(AuthorizeCodeErrorCode.InvalidRequest).WithMessage(AuthorizeCodeErrorDescription.CodeChallenge);
        RuleFor(x => x.CodeChallengeMethod).Matches("^S256|plain$").WithErrorCode("").WithMessage(AuthorizeCodeErrorDescription.CodeChallengeMethod);
        RuleFor(x => x.Nonce).NotEmpty().WithErrorCode(AuthorizeCodeErrorCode.InvalidRequest).WithMessage(AuthorizeCodeErrorDescription.Nonce);
        RuleFor(x => x.Display).Matches("page|popup|touch|wap").WithErrorCode(AuthorizeCodeErrorCode.InvalidRequest)
            .WithMessage(AuthorizeCodeErrorDescription.Display);
    }
}