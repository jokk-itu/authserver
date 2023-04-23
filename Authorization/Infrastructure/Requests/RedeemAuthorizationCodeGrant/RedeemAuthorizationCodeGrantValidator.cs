using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Application;
using Application.Validation;
using Domain;
using Domain.Constants;
using Domain.Enums;
using Infrastructure.Decoders.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Requests.RedeemAuthorizationCodeGrant;
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
    var code = _codeDecoder.DecodeAuthorizationCode(value.Code);

    if (IsCodeVerifierInvalid(value, code.CodeChallenge))
    {
      return new ValidationResult(ErrorCode.InvalidRequest, "code_verifier is invalid", HttpStatusCode.BadRequest);
    }

    if (!string.IsNullOrWhiteSpace(value.Scope) && value.Scope.Split(' ').Except(code.Scopes).Any())
    { 
      return new ValidationResult(ErrorCode.InvalidScope, "scope is not equal to grant", HttpStatusCode.BadRequest);
    }

    if (value.GrantType != GrantTypeConstants.AuthorizationCode)
    {
      return new ValidationResult(ErrorCode.InvalidRequest, "grant_type must be authorization_code",
          HttpStatusCode.BadRequest);
    }

    var query = await _identityContext
      .Set<AuthorizationCodeGrant>()
      .Where(x => x.Id == code.AuthorizationGrantId)
      .Where(AuthorizationCodeGrant.IsAuthorizationCodeValid(code.AuthorizationCodeId))
      .Select(x => new
      {
        IsClientIdValid = x.Client.Id == value.ClientId,
        IsClientSecretValid = x.Client.Secret == value.ClientSecret,
        HasClientSecret = x.Client.Secret  == null,
        IsClientAuthorized = x.Client.RedirectUris.Any(y => y.Uri == value.RedirectUri)
                             && x.Client.GrantTypes.Any(y => y.Name == GrantTypeConstants.AuthorizationCode),
        IsSessionValid = !x.Session.IsRevoked
      })
      .SingleOrDefaultAsync(cancellationToken: cancellationToken);

    if (query is null)
    {
      return new ValidationResult(ErrorCode.InvalidGrant, "grant is invalid", HttpStatusCode.BadRequest);
    }

    if (!query.IsClientIdValid)
    {
      return new ValidationResult(ErrorCode.InvalidClient, "client_id is invalid", HttpStatusCode.BadRequest);
    }

    if (!query.HasClientSecret && !query.IsClientSecretValid)
    {
      return new ValidationResult(ErrorCode.InvalidClient, "client_secret is invalid", HttpStatusCode.BadRequest);
    }

    if (!query.IsClientAuthorized)
    {
      return new ValidationResult(ErrorCode.UnauthorizedClient, "client is unauthorized", HttpStatusCode.BadRequest);
    }

    if (!query.IsSessionValid)
    {
      return new ValidationResult(ErrorCode.InvalidGrant, "grant is invalid", HttpStatusCode.BadRequest);
    }

    return new ValidationResult(HttpStatusCode.OK);
  }

  private static bool IsCodeVerifierInvalid(RedeemAuthorizationCodeGrantCommand command, string codeChallenge)
  {
    var isCodeVerifierInvalid = string.IsNullOrWhiteSpace(command.CodeVerifier) ||
                              !Regex.IsMatch(command.CodeVerifier, "^[0-9a-zA-Z-_~.]{43,128}$");

    if (isCodeVerifierInvalid)
    {
      return true;
    }

    using var sha256 = SHA256.Create();
    var bytes = Encoding.UTF8.GetBytes(command.CodeVerifier);
    var hashed = sha256.ComputeHash(bytes);
    var encoded = Base64UrlEncoder.Encode(hashed);
    return encoded != codeChallenge;
  }
}