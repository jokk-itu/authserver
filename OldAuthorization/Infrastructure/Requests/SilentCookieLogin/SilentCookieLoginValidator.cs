using System.Net;
using Application;
using Application.Validation;
using Domain;
using Infrastructure.Requests.Abstract;
using Infrastructure.Services.Abstract;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Requests.SilentCookieLogin;
public class SilentCookieLoginValidator : AuthorizeValidator, IValidator<SilentCookieLoginCommand>
{
  private readonly IdentityContext _identityContext;

  public SilentCookieLoginValidator(
    IdentityContext identityContext,
    IClientService clientService,
    INonceService nonceService,
    IScopeService scopeService,
    IConsentGrantService consentGrantService)
  : base(clientService, nonceService, scopeService, consentGrantService)
  {
    _identityContext = identityContext;
  }

  public async Task<ValidationResult> ValidateAsync(SilentCookieLoginCommand value, CancellationToken cancellationToken = default)
  {
    var initialValidation = await InitialValidate(value, cancellationToken);
    if (initialValidation.IsError())
    {
      return initialValidation;
    }

    if (string.IsNullOrWhiteSpace(value.UserId))
    {
      return new ValidationResult(
        ErrorCode.InvalidRequest, "user is invalid", HttpStatusCode.OK);
    }

    var validation = await BaseValidate(value, value.UserId, cancellationToken);
    if (validation.IsError())
    {
      return validation;
    }

    var session = await _identityContext
      .Set<User>()
      .Where(u => u.Id == value.UserId)
      .SelectMany(u => u.Sessions)
      .Where(s => !s.IsRevoked)
      .SingleOrDefaultAsync(cancellationToken: cancellationToken);

    if (session is null)
    {
      return new ValidationResult(
        ErrorCode.LoginRequired,
        "session is invalid",
        HttpStatusCode.OK);
    }

    return ValidationResult.OK();
  }
}
