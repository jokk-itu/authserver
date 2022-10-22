using System.Net;
using Application;
using Application.Validation;
using Domain;
using Infrastructure.Validators;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Requests.CreateConsentGrant;
public class CreateConsentGrantValidator : IValidator<CreateConsentGrantCommand>
{
  private readonly IdentityContext _identityContext;
  private readonly UserManager<User> _userManager;

  public CreateConsentGrantValidator(
    IdentityContext identityContext,
    UserManager<User> userManager)
  {
    _identityContext = identityContext;
    _userManager = userManager;
  }

  public async Task<ValidationResult> ValidateAsync(CreateConsentGrantCommand value, CancellationToken cancellationToken = default)
  {
    var clientValidation = await new ClientValidator(_identityContext).ValidateAsync(new ClientToValidate(value.ClientId, null), cancellationToken);
    if(clientValidation.IsError())
      return new ValidationResult(clientValidation.ErrorCode, clientValidation.ErrorDescription, HttpStatusCode.BadRequest);

    var userValidation = await new UserValidator(_userManager).ValidateAsync(new UserToValidate(value.Username, value.Password), cancellationToken);
    if (userValidation.IsError())
      return new ValidationResult(userValidation.ErrorCode, userValidation.ErrorDescription, HttpStatusCode.BadRequest);

    var scopeValidation = await new OpenIdScopeValidator(_identityContext).ValidateAsync(value.Scopes, cancellationToken);
    if (scopeValidation.IsError())
      return new ValidationResult(scopeValidation.ErrorCode, scopeValidation.ErrorDescription, HttpStatusCode.BadRequest);

    if (value.MaxAge < 0)
      return new ValidationResult(ErrorCode.InvalidRequest, "max_age is invalid", HttpStatusCode.BadRequest);

    var claimValidation = await new ClaimValidator(_identityContext).ValidateAsync(value.ConsentedClaims, cancellationToken);
    if (claimValidation.IsError())
      return new ValidationResult(claimValidation.ErrorCode, claimValidation.ErrorDescription, HttpStatusCode.BadRequest);

    return new ValidationResult(HttpStatusCode.OK);
  }
}