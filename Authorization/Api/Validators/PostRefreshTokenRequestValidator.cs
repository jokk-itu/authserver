using Contracts.PostRefreshToken;
using FluentValidation;

namespace Api.Validators;

public class PostRefreshTokenRequestValidator : AbstractValidator<PostRefreshTokenRequest>
{
  public PostRefreshTokenRequestValidator()
  {
    RuleFor(x => x.RefreshToken).NotEmpty();
    RuleFor(x => x.Scope).NotEmpty();
    RuleFor(x => x.GrantType).Equal("refresh_token");
  }
}
