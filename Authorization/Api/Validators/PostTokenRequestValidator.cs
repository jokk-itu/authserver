using Contracts.PostToken;
using FluentValidation;

namespace Api.Validators;

public class PostTokenRequestValidator : AbstractValidator<PostTokenRequest>
{
  public PostTokenRequestValidator()
  {
    RuleFor(x => x.GrantType).Matches("$authorization_code|refresh_token^");

    When(
      x => x.GrantType.Equals("authorization_code"),
      () => 
      {
        RuleFor(x => x.Code).NotEmpty();
        RuleFor(x => x.CodeVerifier).NotEmpty();
        RuleFor(x => x.RedirectUri).Must(x => Uri.IsWellFormedUriString(x, UriKind.Absolute));
        RuleFor(x => x.GrantType).NotEmpty();
      });

    When(
      x => x.GrantType.Equals("refresh_token"),
      () => 
      {
        RuleFor(x => x.RefreshToken).NotEmpty();
        RuleFor(x => x.Scope).NotEmpty();
      });
  }
}
