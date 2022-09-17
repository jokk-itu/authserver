using System.Net;
using System.Text.RegularExpressions;
using Application;
using Application.Validation;
using Domain;
using Domain.Constants;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Requests.CreateClient;
public class CreateClientValidator : IValidator<CreateClientCommand>
{
  private readonly IdentityContext _identityContext;

  public CreateClientValidator(IdentityContext identityContext)
  {
    _identityContext = identityContext;
  }

  public async Task<ValidationResult> IsValidAsync(CreateClientCommand value)
  {
    if (string.IsNullOrWhiteSpace(value.ApplicationType))
      value.ApplicationType = "web";

    if (!Regex.IsMatch(value.ApplicationType, "^web|native$"))
      return GetInvalidClientMetadataResult("application_type is invalid");

    if (string.IsNullOrWhiteSpace(value.ClientName))
      return GetInvalidClientMetadataResult("client_name is required");

    if (string.IsNullOrWhiteSpace(value.ClientSecret))
      return GetInvalidClientMetadataResult("client_secret is required");

    if(!value.RedirectUris.Any())
      return GetInvalidClientMetadataResult("application_type is invalid");

    foreach (var redirectUri in value.RedirectUris)
    {
      if (!Uri.IsWellFormedUriString(redirectUri, UriKind.Absolute))
        return GetInvalidClientMetadataResult($"redirect_uri {redirectUri} must be a well formed uri");
    }

    if(!value.ResponseTypes.Any())
      value.ResponseTypes.Add(ResponseTypeConstants.Code);

    foreach (var responseType in value.ResponseTypes)
    {
      if (!ResponseTypeConstants.ResponseTypes.Contains(responseType))
        return GetInvalidClientMetadataResult($"response_type {responseType} is not valid");
    }

    if (!value.GrantTypes.Any())
      return GetInvalidClientMetadataResult("at least one grant_type is required");

    var grants = await _identityContext
      .Set<Grant>()
      .IgnoreAutoIncludes()
      .ToListAsync();

    foreach (var grantType in value.GrantTypes)
    {
      if (grants.All(x => x.Name != grantType))
        return GetInvalidClientMetadataResult($"grant_type {grantType} is invalid");
    }

    foreach (var contact in value.Contacts)
    {
      var ampersandPosition = contact.LastIndexOf('@');
      var isValidContact = ampersandPosition > 0 
                           && (contact.LastIndexOf(".", StringComparison.Ordinal) > ampersandPosition) 
                           && (contact.Length - ampersandPosition > 4);
      if (!isValidContact)
        return GetInvalidClientMetadataResult($"contact {contact} is not a valid email address");
    }

    if (!value.Scopes.Contains(ScopeConstants.OpenId))
      return GetInvalidClientMetadataResult($"scope must contain {ScopeConstants.OpenId}");

    if (!string.IsNullOrWhiteSpace(value.PolicyUri) && !Uri.IsWellFormedUriString(value.PolicyUri, UriKind.Absolute))
      return GetInvalidClientMetadataResult("policy_uri must be a well formed uri");

    if (!string.IsNullOrWhiteSpace(value.TosUri) && !Uri.IsWellFormedUriString(value.TosUri, UriKind.Absolute))
      return GetInvalidClientMetadataResult("tos_uri must be a well formed uri");

    if (!string.IsNullOrWhiteSpace(value.SubjectType) && !SubjectTypeConstants.SubjectTypes.Contains(value.SubjectType))
      return GetInvalidClientMetadataResult("subject_type is invalid");

    if (!string.IsNullOrWhiteSpace(value.TokenEndpointAuthMethod) 
        && !TokenEndpointAuthMethodConstants.TokenEndpointAuthMethods.Contains(value.TokenEndpointAuthMethod))
      return GetInvalidClientMetadataResult($"token_endpoint_auth_method {value.TokenEndpointAuthMethod} is invalid");

    return new ValidationResult(HttpStatusCode.OK);
  }

  private static ValidationResult GetInvalidClientMetadataResult(string errorDescription)
  {
    return new ValidationResult(ErrorCode.InvalidClientMetadata, errorDescription, HttpStatusCode.BadRequest);
  }
}
