using System.Net;
using Application;
using Application.Validation;
using Domain;
using Domain.Constants;
using Infrastructure.Services.Abstract;
using Infrastructure.Validators;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Requests.SilentCookieLogin;
public class SilentCookieLoginValidator : IValidator<SilentCookieLoginCommand>
{
  private readonly IdentityContext _identityContext;
  private readonly IClientService _clientService;
  private readonly INonceService _nonceService;
  private readonly IScopeService _scopeService;
  private readonly IConsentGrantService _consentGrantService;

  public SilentCookieLoginValidator(
    IdentityContext identityContext,
    IClientService clientService,
    INonceService nonceService,
    IScopeService scopeService,
    IConsentGrantService consentGrantService)
  {
    _identityContext = identityContext;
    _clientService = clientService;
    _nonceService = nonceService;
    _scopeService = scopeService;
    _consentGrantService = consentGrantService;
  }

  public async Task<ValidationResult> ValidateAsync(SilentCookieLoginCommand value, CancellationToken cancellationToken = default)
  {
    var redirectAuthorizationValidation = await _clientService.ValidateRedirectAuthorization(
      value.ClientId, value.RedirectUri, value.State, cancellationToken);
    
    if (redirectAuthorizationValidation.IsError())
    {
      return new ValidationResult(
        redirectAuthorizationValidation.ErrorCode,
        redirectAuthorizationValidation.ErrorDescription,
        HttpStatusCode.BadRequest);
    }

    if (string.IsNullOrWhiteSpace(value.UserId))
    {
      return new ValidationResult(
        ErrorCode.InvalidRequest, "user is invalid", HttpStatusCode.OK);
    }

    var nonceValidationResult = await _nonceService.ValidateNonce(value.Nonce, cancellationToken);
    if (nonceValidationResult.IsError())
    {
      return new ValidationResult(
        nonceValidationResult.ErrorCode,
        nonceValidationResult.ErrorDescription,
        HttpStatusCode.OK);
    }

    var scopeValidationResult = await _scopeService.ValidateScope(value.Scope, cancellationToken);
    if (scopeValidationResult.IsError())
    {
      return new ValidationResult(
        scopeValidationResult.ErrorCode,
        scopeValidationResult.ErrorDescription,
        HttpStatusCode.OK);
    }

    if (value.ResponseType != ResponseTypeConstants.Code)
    {
      return new ValidationResult(
        ErrorCode.UnsupportedResponseType,
        "response_type must be code",
        HttpStatusCode.OK);
    }

    var codeChallengeMethodValidation = PkceValidator.ValidateCodeChallengeMethod(value.CodeChallengeMethod);
    if (codeChallengeMethodValidation.IsError())
    {
      return new ValidationResult(
        codeChallengeMethodValidation.ErrorCode,
        codeChallengeMethodValidation.ErrorDescription,
        HttpStatusCode.OK);
    }

    var codeChallengeValidation = PkceValidator.ValidateCodeChallenge(value.CodeChallenge);
    if (codeChallengeValidation.IsError())
    {
      return new ValidationResult(
        codeChallengeValidation.ErrorCode,
        codeChallengeValidation.ErrorDescription,
        HttpStatusCode.OK);
    }

    var clientAuthorizationValidationResult =
      await _clientService.ValidateClientAuthorization(value.Scope, value.ClientId, cancellationToken);

    if (clientAuthorizationValidationResult.IsError())
    {
      return new ValidationResult(
        clientAuthorizationValidationResult.ErrorCode,
        clientAuthorizationValidationResult.ErrorDescription,
        HttpStatusCode.OK);
    }

    var consentGrantValidation = await _consentGrantService.ValidateConsentedScopes(
      value.UserId, value.ClientId, value.Scope, cancellationToken);

    if (consentGrantValidation.IsError())
    {
      return new ValidationResult(
        consentGrantValidation.ErrorCode,
        consentGrantValidation.ErrorDescription,
        HttpStatusCode.OK);
    }

    var maxAgeValidation = MaxAgeValidator.ValidateMaxAge(value.MaxAge);
    if (maxAgeValidation.IsError())
    {
      return new ValidationResult(
        maxAgeValidation.ErrorCode,
        maxAgeValidation.ErrorDescription,
        HttpStatusCode.OK);
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

    return new ValidationResult(HttpStatusCode.OK);
  }
}
