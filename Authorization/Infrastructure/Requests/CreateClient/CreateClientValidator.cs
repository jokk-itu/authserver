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
    if (IsApplicationTypeInvalid(value))
      return GetInvalidClientMetadataResult("application_type is invalid");

    if (string.IsNullOrWhiteSpace(value.ClientName))
      return GetInvalidClientMetadataResult("client_name is required");

    if (IsRedirectUrisInvalid(value))
      return GetInvalidClientMetadataResult("redirect_uri is invalid");

    if (IsResponseTypesInvalid(value))
      return GetInvalidClientMetadataResult("response_type is invalid");

    if (await IsGrantTypeInvalidAsync(value))
      return GetInvalidClientMetadataResult("grant_type is invalid");

    if (IsContactsInvalid(value))
      return GetInvalidClientMetadataResult("contacts is invalid");

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

  private static bool IsApplicationTypeInvalid(CreateClientCommand command)
  {
    if (string.IsNullOrWhiteSpace(command.ApplicationType))
      command.ApplicationType = "web";

    return !Regex.IsMatch(command.ApplicationType, "^web|native$");
  }

  private static bool IsRedirectUrisInvalid(CreateClientCommand command)
  {
    return !command.RedirectUris.Any() || command.RedirectUris.Any(redirectUri => !Uri.IsWellFormedUriString(redirectUri, UriKind.Absolute));
  }

  private static bool IsResponseTypesInvalid(CreateClientCommand command)
  {
    if(!command.ResponseTypes.Any())
      command.ResponseTypes.Add(ResponseTypeConstants.Code);

    return command.ResponseTypes.Any(responseType => !ResponseTypeConstants.ResponseTypes.Contains(responseType));
  }

  private async Task<bool> IsGrantTypeInvalidAsync(CreateClientCommand command)
  {
    if (!command.GrantTypes.Any())
      return true;

    var grants = await _identityContext
      .Set<Grant>()
      .IgnoreAutoIncludes()
      .ToListAsync();

    return command.GrantTypes.Any(grantType => grants.All(x => x.Name != grantType));
  }

  private static bool IsContactsInvalid(CreateClientCommand command)
  {
    return (
      from contact in command.Contacts 
      let ampersandPosition = contact.LastIndexOf('@') 
      select ampersandPosition > 0 
             && (contact.LastIndexOf(".", StringComparison.Ordinal) > ampersandPosition) 
             && (contact.Length - ampersandPosition > 4)).Any(isValidContact => !isValidContact);
  }

  private static ValidationResult GetInvalidClientMetadataResult(string errorDescription)
  {
    return new ValidationResult(ErrorCode.InvalidClientMetadata, errorDescription, HttpStatusCode.BadRequest);
  }
}
