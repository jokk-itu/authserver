using System.Net;
using Application.Validation;
using Infrastructure.Requests.Abstract;
using Infrastructure.Services.Abstract;

namespace Infrastructure.Requests.CreateAuthorizationGrant;

public class CreateAuthorizationGrantValidator : AuthorizeValidator, IValidator<CreateAuthorizationGrantCommand>
{
  public CreateAuthorizationGrantValidator(
    IClientService clientService,
    INonceService nonceService,
    IScopeService scopeService,
    IConsentGrantService consentGrantService)
  : base (clientService, nonceService, scopeService, consentGrantService)
  {
  }

  public async Task<ValidationResult> ValidateAsync(CreateAuthorizationGrantCommand value,
    CancellationToken cancellationToken = default)
  {
    var initialValidation = await InitialValidate(value, cancellationToken);
    if (initialValidation.IsError())
    {
      return initialValidation;
    }

    var validation = await BaseValidate(value, value.UserId, cancellationToken);
    if (validation.IsError())
    {
      return validation;
    }

    return new ValidationResult(HttpStatusCode.OK);
  }
}