using System.Net;
using Application;
using Application.Validation;
using Domain;
using Domain.Constants;
using Infrastructure.Decoders.Abstractions;
using Infrastructure.Helpers;
using Infrastructure.Services.Abstract;
using Infrastructure.Validators;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Requests.RedeemAuthorizationCodeGrant;

public class RedeemAuthorizationCodeGrantValidator : IValidator<RedeemAuthorizationCodeGrantCommand>
{
  private readonly IdentityContext _identityContext;
  private readonly ICodeDecoder _codeDecoder;
  private readonly IResourceService _resourceService;

  public RedeemAuthorizationCodeGrantValidator(
    IdentityContext identityContext,
    ICodeDecoder codeDecoder,
    IResourceService resourceService)
  {
    _identityContext = identityContext;
    _codeDecoder = codeDecoder;
    _resourceService = resourceService;
  }

  public async Task<ValidationResult> ValidateAsync(RedeemAuthorizationCodeGrantCommand value, CancellationToken cancellationToken = default)
  { 
    var code = _codeDecoder.DecodeAuthorizationCode(value.Code);

    var codeVerifierValidation = PkceValidator.ValidateCodeVerifier(value.CodeVerifier, code.CodeChallenge);
    if (codeVerifierValidation.IsError())
    {
      return new ValidationResult(codeVerifierValidation.ErrorCode, codeVerifierValidation.ErrorDescription, HttpStatusCode.BadRequest);
    }

    if (value.GrantType != GrantTypeConstants.AuthorizationCode)
    {
      return new ValidationResult(ErrorCode.InvalidRequest, "grant_type must be authorization_code",
          HttpStatusCode.BadRequest);
    }

    if (!string.IsNullOrEmpty(code.RedirectUri) && value.RedirectUri != code.RedirectUri)
    {
      return new ValidationResult(ErrorCode.InvalidRequest, "redirect_uri is invalid",
        HttpStatusCode.BadRequest);
    }

    if (value.ClientAuthentications.Count != 1)
    {
      return new ValidationResult(ErrorCode.InvalidClient, "multiple or none client authentication methods detected",
        HttpStatusCode.BadRequest);
    }

    var clientAuthentication = value.ClientAuthentications.Single();

    var query = await _identityContext
      .Set<AuthorizationCodeGrant>()
      .Where(x => x.Id == code.AuthorizationGrantId)
      .Where(AuthorizationCodeGrant.IsAuthorizationCodeValid(code.AuthorizationCodeId))
      .Select(x => new
      {
        IsClientIdValid = x.Client.Id == clientAuthentication.ClientId,
        ClientSecret = x.Client.Secret,
        IsRedirectAuthorized = x.Client.RedirectUris.Any(y => y.Uri == value.RedirectUri),
        IsGrantTypeAuthorized = x.Client.GrantTypes.Any(y => y.Name == GrantTypeConstants.AuthorizationCode),
        IsSessionValid = !x.Session.IsRevoked,
        UserId = x.Session.User.Id
      })
      .SingleOrDefaultAsync(cancellationToken: cancellationToken);

    if (query is null)
    {
      return new ValidationResult(ErrorCode.InvalidGrant, "grant is invalid", HttpStatusCode.BadRequest);
    }

    var isClientSecretValid = query.ClientSecret == null
                              || !string.IsNullOrWhiteSpace(clientAuthentication.ClientSecret)
                              && BCrypt.CheckPassword(clientAuthentication.ClientSecret, query.ClientSecret);

    if (!query.IsClientIdValid || !isClientSecretValid)
    {
      return new ValidationResult(ErrorCode.InvalidClient, "client could not be authenticated",
        HttpStatusCode.BadRequest);
    }

    if (!string.IsNullOrEmpty(value.RedirectUri) && !query.IsRedirectAuthorized)
    {
      return new ValidationResult(ErrorCode.UnauthorizedClient, "client is unauthorized for redirect_uri", HttpStatusCode.BadRequest);
    }

    if (!query.IsGrantTypeAuthorized)
    {
      return new ValidationResult(ErrorCode.UnauthorizedClient, "client is unauthorized for grant_type", HttpStatusCode.BadRequest);
    }

    if (!query.IsSessionValid)
    {
      return new ValidationResult(ErrorCode.InvalidGrant, "grant is invalid", HttpStatusCode.BadRequest);
    }

    var consentGrant = await _identityContext
      .Set<ConsentGrant>()
      .Where(x => x.User.Id == query.UserId)
      .Where(x => x.Client.Id == clientAuthentication.ClientId)
      .Include(x => x.ConsentedScopes)
      .SingleOrDefaultAsync(cancellationToken: cancellationToken);

    if (consentGrant is null)
    {
      return new ValidationResult(ErrorCode.ConsentRequired, "consent is required", HttpStatusCode.BadRequest);
    }

    if (code.Scopes.Except(consentGrant.ConsentedScopes.Select(x => x.Name)).Any())
    {
      return new ValidationResult(ErrorCode.InvalidScope, "scope exceeds consented scope", HttpStatusCode.BadRequest);
    }

    var scope = string.Join(' ', code.Scopes);
    var resourceValidation = await _resourceService.ValidateResources(value.Resource, scope);
    if (resourceValidation.IsError())
    {
      return new ValidationResult(
        resourceValidation.ErrorCode,
        resourceValidation.ErrorDescription,
        HttpStatusCode.OK);
    }

    return new ValidationResult(HttpStatusCode.OK);
  }
}