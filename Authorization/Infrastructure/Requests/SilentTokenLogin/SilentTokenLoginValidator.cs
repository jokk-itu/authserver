using System.IdentityModel.Tokens.Jwt;
using System.Net;
using Application;
using Application.Validation;
using Domain;
using Domain.Constants;
using Infrastructure.Decoders.Token;
using Infrastructure.Decoders.Token.Abstractions;
using Infrastructure.Services.Abstract;
using Infrastructure.Validators;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Requests.SilentTokenLogin;

public class SilentTokenLoginValidator : IValidator<SilentTokenLoginCommand>
{
  private readonly IdentityContext _identityContext;
  private readonly IStructuredTokenDecoder _tokenDecoder;
  private readonly IClientService _clientService;
  private readonly INonceService _nonceService;
  private readonly IScopeService _scopeService;
  private readonly IConsentGrantService _consentGrantService;

  public SilentTokenLoginValidator(
    IdentityContext identityContext,
    IStructuredTokenDecoder tokenDecoder,
    IClientService clientService,
    INonceService nonceService,
    IScopeService scopeService,
    IConsentGrantService consentGrantService)
  {
    _identityContext = identityContext;
    _tokenDecoder = tokenDecoder;
    _clientService = clientService;
    _nonceService = nonceService;
    _scopeService = scopeService;
    _consentGrantService = consentGrantService;
  }

  public async Task<ValidationResult> ValidateAsync(SilentTokenLoginCommand value,
    CancellationToken cancellationToken = default)
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

    var (token, validationResult) = await GetValidatedToken(value);

    if (validationResult?.IsError() ?? false)
    {
      return validationResult;
    }

    var userId = token?.Claims.SingleOrDefault(x => x.Type == ClaimNameConstants.Sub)?.Value 
                 ?? throw new InvalidOperationException($"{ClaimNameConstants.Sub} cannot be null");

    var authorizationGrantId = token?.Claims.SingleOrDefault(x => x.Type == ClaimNameConstants.GrantId)?.Value
      ?? throw new InvalidOperationException($"{ClaimNameConstants.GrantId} cannot be null");

    var sessionId = token?.Claims.SingleOrDefault(x => x.Type == ClaimNameConstants.Sid)?.Value
      ?? throw new InvalidOperationException($"{ClaimNameConstants.Sid} cannot be null");

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
      userId, value.ClientId, value.Scope, cancellationToken);

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

    var isSessionValid = await _identityContext
      .Set<Session>()
      .Where(x => x.Id == sessionId)
      .Where(x => !x.IsRevoked)
      .AnyAsync(cancellationToken: cancellationToken);

    if (!isSessionValid)
    {
      return new ValidationResult(
        ErrorCode.LoginRequired,
        "session is not valid", HttpStatusCode.OK);
    }

    return new ValidationResult(HttpStatusCode.OK);
  }

  private async Task<(JwtSecurityToken?, ValidationResult?)> GetValidatedToken(SilentTokenLoginCommand command)
  {
    try
    {
      var token = await _tokenDecoder.Decode(command.IdTokenHint, new StructuredTokenDecoderArguments
      {
        ClientId = command.ClientId,
        Audiences = new[] { command.ClientId },
        ValidateAudience = true,
        ValidateLifetime = true
      });

      return (token, null);
    }
    catch (Exception)
    {
      return (null, new ValidationResult(
        ErrorCode.LoginRequired,
            "user must be authenticated",
            HttpStatusCode.OK));
    }
  }
}