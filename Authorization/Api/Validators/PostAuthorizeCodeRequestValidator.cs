using AuthorizationServer.Constants;
using Contracts.AuthorizeCode;
using FluentValidation;
using FluentValidation.Results;

namespace AuthorizationServer.Validators;

public class PostAuthorizeRequestValidator : AbstractValidator<PostAuthorizeCodeRequest>
{
  public PostAuthorizeRequestValidator()
  {
    RuleFor(x => x.Username).NotNull().WithErrorCode(AuthorizeCodeErrorCode.InvalidRequest).WithMessage(AuthorizeCodeErrorDescription.Username);
    RuleFor(x => x.Password).NotNull().WithErrorCode(AuthorizeCodeErrorCode.InvalidRequest).WithMessage(AuthorizeCodeErrorDescription.Password);
    RuleFor(x => x.ResponseType)
        .Matches(
            $"^({ResponseType.Code} {ResponseType.IdToken})|({ResponseType.Code})|({ResponseType.IdToken} {ResponseType.Code})$")
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
    RuleFor(x => x.CodeChallengeMethod).Matches("^S256$").WithErrorCode(AuthorizeCodeErrorCode.InvalidRequest).WithMessage(AuthorizeCodeErrorDescription.CodeChallengeMethod);
  }
}