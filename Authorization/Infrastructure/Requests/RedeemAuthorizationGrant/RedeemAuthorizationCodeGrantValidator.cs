using System.Net;
using System.Text.RegularExpressions;
using Application;
using Application.Validation;
using Domain;
using Infrastructure.Factories;
using Infrastructure.Requests.CreateAuthorizationCodeGrant;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Requests.RedeemAuthorizationGrant;
public class RedeemAuthorizationCodeGrantValidator : IValidator<RedeemAuthorizationCodeGrantCommand>
{
  private readonly IdentityContext _identityContext;
  private readonly CodeFactory _codeFactory;

  public RedeemAuthorizationCodeGrantValidator(
    IdentityContext identityContext, 
    CodeFactory codeFactory)
  {
    _identityContext = identityContext;
    _codeFactory = codeFactory;
  }

  public async Task<ValidationResult> ValidateAsync(RedeemAuthorizationCodeGrantCommand value, CancellationToken cancellationToken = default)
  {
    if (await IsClientUnauthorizedAsync(value))
      return new ValidationResult(ErrorCode.InvalidClient, "client is invalid", HttpStatusCode.BadRequest);

    if (IsCodeVerifierInvalid(value))
      return new ValidationResult(ErrorCode.InvalidRequest, "invalid code_verifier", HttpStatusCode.BadRequest);

    if (await IsCodeInvalidAsync(value))
      return new ValidationResult(ErrorCode.InvalidRequest, "code is invalid", HttpStatusCode.BadRequest);

    if (IsScopeInvalid(value))
      return new ValidationResult(ErrorCode.InvalidScope, "scope is invalid", HttpStatusCode.BadRequest);

    return new ValidationResult(HttpStatusCode.OK);
  }

  private async Task<bool> IsClientUnauthorizedAsync(RedeemAuthorizationCodeGrantCommand command)
  {
    if (string.IsNullOrWhiteSpace(command.ClientId)
        || string.IsNullOrWhiteSpace(command.ClientSecret)
        || string.IsNullOrWhiteSpace(command.RedirectUri))
      return true;

    var client = await _identityContext
      .Set<Client>()
      .SingleOrDefaultAsync(x => x.Id == command.ClientId 
                                 && x.Secret == command.ClientSecret 
                                 && x.RedirectUris.Any(y => y.Uri == command.RedirectUri));
    return client is null;
  }

  private static bool IsCodeVerifierInvalid(RedeemAuthorizationCodeGrantCommand command)
  {
    return string.IsNullOrWhiteSpace(command.CodeVerifier) ||
        !Regex.IsMatch(command.CodeVerifier, "^[0-9a-zA-Z-_~.]{43,128}$");
  }

  private async Task<bool> IsCodeInvalidAsync(RedeemAuthorizationCodeGrantCommand command)
  {
    if (string.IsNullOrWhiteSpace(command.Code))
      return true;

    if (await _identityContext
          .Set<Code>()
          .AnyAsync(x => x.Value == command.Code))
      return true;

    if (await _codeFactory.ValidateAsync(command.Code, command.RedirectUri, command.ClientId, command.CodeVerifier))
      return true;

    return false;
  }

  private bool IsScopeInvalid(RedeemAuthorizationCodeGrantCommand command)
  {
    var code = _codeFactory.DecodeCode(command.Code);
    if (code is null)
      return true;

    if (string.IsNullOrWhiteSpace(command.Scope))
      return true;

    var requestScopes = command.Scope.Split(' ');
    return !requestScopes.All(x => code.Scopes.Contains(x));
  }
}
