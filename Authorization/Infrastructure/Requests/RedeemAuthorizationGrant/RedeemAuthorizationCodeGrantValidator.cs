using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Application;
using Application.Validation;
using Domain;
using Domain.Constants;
using Infrastructure.Decoders.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Requests.RedeemAuthorizationGrant;
public class RedeemAuthorizationCodeGrantValidator : IValidator<RedeemAuthorizationCodeGrantCommand>
{
  private readonly IdentityContext _identityContext;
  private readonly ICodeDecoder _codeDecoder;

  public RedeemAuthorizationCodeGrantValidator(
    IdentityContext identityContext,
    ICodeDecoder codeDecoder)
  {
    _identityContext = identityContext;
    _codeDecoder = codeDecoder;
  }

  public async Task<ValidationResult> ValidateAsync(RedeemAuthorizationCodeGrantCommand value, CancellationToken cancellationToken = default)
  {
    if (await IsClientInvalidAsync(value))
      return new ValidationResult(ErrorCode.InvalidClient, "client is invalid", HttpStatusCode.BadRequest);

    if (await IsClientUnauthorizedAsync(value))
      return new ValidationResult(ErrorCode.UnauthorizedClient, "client is unauthorized", HttpStatusCode.Unauthorized);

    if (IsCodeVerifierInvalid(value))
      return new ValidationResult(ErrorCode.InvalidRequest, "invalid code_verifier", HttpStatusCode.BadRequest);

    if (await IsCodeInvalidAsync(value))
      return new ValidationResult(ErrorCode.InvalidRequest, "code is invalid", HttpStatusCode.BadRequest);

    if (await IsScopeInvalidAsync(value))
      return new ValidationResult(ErrorCode.InvalidScope, "scope is invalid", HttpStatusCode.BadRequest);

    if (await IsSessionInvalidAsync(value))
      return new ValidationResult(ErrorCode.AccessDenied, "session is invalid", HttpStatusCode.Unauthorized);

    return new ValidationResult(HttpStatusCode.OK);
  }

  private async Task<bool> IsClientInvalidAsync(RedeemAuthorizationCodeGrantCommand command)
  {
    if (string.IsNullOrWhiteSpace(command.ClientId)
        || string.IsNullOrWhiteSpace(command.ClientSecret))
      return true;

    var client = await _identityContext
      .Set<Client>()
      .SingleOrDefaultAsync(x => x.Id == command.ClientId && x.Secret == command.ClientSecret);

    return client is null;
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
                                 && x.RedirectUris.Any(y => y.Uri == command.RedirectUri)
                                 && x.GrantTypes.Any(y => y.Name == GrantTypeConstants.AuthorizationCode));
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

    var code = _codeDecoder.DecodeAuthorizationCode(command.Code);
    var authorizationGrant = await _identityContext
      .Set<AuthorizationCodeGrant>()
      .SingleOrDefaultAsync(x => x.Id == code.AuthorizationGrantId);

    if (authorizationGrant is null)
      return true;

    if (authorizationGrant.IsRedeemed)
      return true;

    using var sha256 = SHA256.Create();
    var hashed = sha256.ComputeHash(Encoding.UTF8.GetBytes(command.CodeVerifier));
    var encoded = Base64UrlEncoder.Encode(hashed);
    if (code.CodeChallenge != encoded)
      return true;

    return false;
  }

  private async Task<bool> IsScopeInvalidAsync(RedeemAuthorizationCodeGrantCommand command)
  {
    var code = _codeDecoder.DecodeAuthorizationCode(command.Code);
    var authorizationGrant = await _identityContext
      .Set<AuthorizationCodeGrant>()
      .SingleOrDefaultAsync(x => x.Id == code.AuthorizationGrantId);

    if (authorizationGrant is null)
      return true;

    if (string.IsNullOrWhiteSpace(command.Scope))
      return false;
    
    var requestScopes = command.Scope.Split(' ');
    return !requestScopes.All(x => code.Scopes.Any(y => y == x));
  }

  private async Task<bool> IsSessionInvalidAsync(RedeemAuthorizationCodeGrantCommand command)
  {
    var code = _codeDecoder.DecodeAuthorizationCode(command.Code);
    var session = await _identityContext
      .Set<Session>()
      .SingleOrDefaultAsync(x => x.User.Id == code.UserId);

    return session is null;
  }
}