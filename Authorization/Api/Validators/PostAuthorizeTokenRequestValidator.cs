using Contracts.PostAuthorizeToken;
using FluentValidation;

namespace Api.Validators;

public class PostAuthorizeTokenRequestValidator : AbstractValidator<PostAuthorizeTokenRequest>
{
  public PostAuthorizeTokenRequestValidator()
  {
    RuleFor(x => x.Code).NotEmpty();
    RuleFor(x => x.CodeVerifier).NotEmpty();
    RuleFor(x => x.RedirectUri).Must(x => Uri.IsWellFormedUriString(x, UriKind.Absolute));
    RuleFor(x => x.GrantType).Equal("authorization_code");
    RuleFor(x => x.GrantType).NotEmpty();
  }
}
