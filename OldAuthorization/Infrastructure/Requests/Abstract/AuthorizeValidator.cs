using Application;
using Application.Validation;
using Domain.Constants;
using Infrastructure.Services.Abstract;
using Infrastructure.Validators;
using System.Net;

namespace Infrastructure.Requests.Abstract;
public abstract class AuthorizeValidator
{
  private readonly IClientService _clientService;
  private readonly INonceService _nonceService;
  private readonly IScopeService _scopeService;
  private readonly IConsentGrantService _consentGrantService;

  protected AuthorizeValidator(
    IClientService clientService,
    INonceService nonceService,
    IScopeService scopeService,
    IConsentGrantService consentGrantService)
  {
    _clientService = clientService;
    _nonceService = nonceService;
    _scopeService = scopeService;
    _consentGrantService = consentGrantService;
  }

  protected async Task<ValidationResult> InitialValidate(AuthorizeRequest request, CancellationToken cancellationToken)
  {
    var redirectAuthorizationValidation = await _clientService.ValidateRedirectAuthorization(
      request.ClientId, request.RedirectUri, request.State, cancellationToken);

    if (redirectAuthorizationValidation.IsError())
    {
      return new ValidationResult(
        redirectAuthorizationValidation.ErrorCode,
        redirectAuthorizationValidation.ErrorDescription,
        HttpStatusCode.BadRequest);
    }

    return ValidationResult.OK();
  }

  protected async Task<ValidationResult> BaseValidate(AuthorizeRequest request, string userId, CancellationToken cancellationToken)
  {
    var nonceValidationResult = await _nonceService.ValidateNonce(request.Nonce, cancellationToken);
    if (nonceValidationResult.IsError())
    {
      return new ValidationResult(
        nonceValidationResult.ErrorCode,
        nonceValidationResult.ErrorDescription,
        HttpStatusCode.OK);
    }

    var scopeValidationResult = await _scopeService.ValidateScope(request.Scope, cancellationToken);
    if (scopeValidationResult.IsError())
    {
      return new ValidationResult(
        scopeValidationResult.ErrorCode,
        scopeValidationResult.ErrorDescription,
        HttpStatusCode.OK);
    }

    if (request.ResponseType != ResponseTypeConstants.Code)
    {
      return new ValidationResult(
        ErrorCode.UnsupportedResponseType,
        "response_type must be code",
        HttpStatusCode.OK);
    }

    var codeChallengeMethodValidation = PkceValidator.ValidateCodeChallengeMethod(request.CodeChallengeMethod);
    if (codeChallengeMethodValidation.IsError())
    {
      return new ValidationResult(
        codeChallengeMethodValidation.ErrorCode,
        codeChallengeMethodValidation.ErrorDescription,
        HttpStatusCode.OK);
    }

    var codeChallengeValidation = PkceValidator.ValidateCodeChallenge(request.CodeChallenge);
    if (codeChallengeValidation.IsError())
    {
      return new ValidationResult(
        codeChallengeValidation.ErrorCode,
        codeChallengeValidation.ErrorDescription,
        HttpStatusCode.OK);
    }

    var clientAuthorizationValidationResult =
      await _clientService.ValidateClientAuthorization(request.Scope, request.ClientId, cancellationToken);

    if (clientAuthorizationValidationResult.IsError())
    {
      return new ValidationResult(
        clientAuthorizationValidationResult.ErrorCode,
        clientAuthorizationValidationResult.ErrorDescription,
        HttpStatusCode.OK);
    }

    var consentGrantValidation = await _consentGrantService.ValidateConsentedScopes(
      userId, request.ClientId, request.Scope, cancellationToken);

    if (consentGrantValidation.IsError())
    {
      return new ValidationResult(
        consentGrantValidation.ErrorCode,
        consentGrantValidation.ErrorDescription,
        HttpStatusCode.OK);
    }

    var maxAgeValidation = MaxAgeValidator.ValidateMaxAge(request.MaxAge);
    if (maxAgeValidation.IsError())
    {
      return new ValidationResult(maxAgeValidation.ErrorCode,
        maxAgeValidation.ErrorDescription, HttpStatusCode.OK);
    }

    return ValidationResult.OK();
  }
}