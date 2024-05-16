using System.IdentityModel.Tokens.Jwt;
using System.Net;
using Application;
using Application.Validation;
using Domain;
using Domain.Constants;
using Infrastructure.Decoders.Token;
using Infrastructure.Decoders.Token.Abstractions;
using Infrastructure.Requests.Abstract;
using Infrastructure.Services.Abstract;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Requests.SilentTokenLogin;

public class SilentTokenLoginValidator : AuthorizeValidator, IValidator<SilentTokenLoginCommand>
{
  private readonly IdentityContext _identityContext;
  private readonly IStructuredTokenDecoder _tokenDecoder;

  public SilentTokenLoginValidator(
    IdentityContext identityContext,
    IStructuredTokenDecoder tokenDecoder,
    IClientService clientService,
    INonceService nonceService,
    IScopeService scopeService,
    IConsentGrantService consentGrantService)
  : base(clientService, nonceService, scopeService, consentGrantService)
  {
    _identityContext = identityContext;
    _tokenDecoder = tokenDecoder;
  }

  public async Task<ValidationResult> ValidateAsync(SilentTokenLoginCommand value,
    CancellationToken cancellationToken = default)
  {
    var initialValidation = await InitialValidate(value, cancellationToken);
    if (initialValidation.IsError())
    {
      return initialValidation;
    }

    var (token, validationResult) = await GetValidatedToken(value);

    if (validationResult?.IsError() ?? false)
    {
      return validationResult;
    }

    var sessionId = token?.Claims.SingleOrDefault(x => x.Type == ClaimNameConstants.Sid)?.Value
                    ?? throw new InvalidOperationException($"{ClaimNameConstants.Sid} cannot be null");

    var userId = token?.Claims.SingleOrDefault(x => x.Type == ClaimNameConstants.Sub)?.Value
                 ?? throw new InvalidOperationException($"{ClaimNameConstants.Sub} cannot be null");

    var validation = await BaseValidate(value, userId, cancellationToken);
    if (validation.IsError())
    {
      return validation;
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