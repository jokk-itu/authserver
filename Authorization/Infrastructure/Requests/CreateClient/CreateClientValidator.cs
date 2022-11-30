using System.Net;
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

  public async Task<ValidationResult> ValidateAsync(CreateClientCommand value, CancellationToken cancellationToken = default)
  {
    if (IsApplicationTypeInvalid(value))
      return GetInvalidClientMetadataResult("application_type is invalid");

    if (await IsClientNameInvalidAsync(value))
      return GetInvalidClientMetadataResult("client_name is required");

    if (IsRedirectUrisInvalid(value))
      return GetInvalidClientMetadataResult("redirect_uri is invalid");

    if (IsResponseTypesInvalid(value))
      return GetInvalidClientMetadataResult("response_type is invalid");

    if (await IsGrantTypeInvalidAsync(value))
      return GetInvalidClientMetadataResult("grant_type is invalid");

    if (IsContactsInvalid(value))
      return GetInvalidClientMetadataResult("contacts is invalid");

    if (await IsScopeInvalidAsync(value))
      return GetInvalidClientMetadataResult("scope is invalid");

    if (!string.IsNullOrWhiteSpace(value.PolicyUri) && !Uri.IsWellFormedUriString(value.PolicyUri, UriKind.Absolute))
      return GetInvalidClientMetadataResult("policy_uri must be a well formed uri");

    if (!string.IsNullOrWhiteSpace(value.TosUri) && !Uri.IsWellFormedUriString(value.TosUri, UriKind.Absolute))
      return GetInvalidClientMetadataResult("tos_uri must be a well formed uri");

    if(IsSubjectTypeInvalid(value))
      return GetInvalidClientMetadataResult("subject_type is invalid");

    if(IsTokenEndpointAuthMethodInvalid(value))
      return GetInvalidClientMetadataResult("token_endpoint_auth_method is invalid");

    return new ValidationResult(HttpStatusCode.OK);
  }

  private static bool IsApplicationTypeInvalid(CreateClientCommand command)
  {
    if (string.IsNullOrWhiteSpace(command.ApplicationType))
      command.ApplicationType = ApplicationTypeConstants.Web;

    return !ApplicationTypeConstants.ApplicationTypes.Contains(command.ApplicationType);
  }

  private async Task<bool> IsClientNameInvalidAsync(CreateClientCommand command)
  {
    if (string.IsNullOrWhiteSpace(command.ClientName))
      return true;

    return await _identityContext
      .Set<Client>()
      .AnyAsync(x => x.Name == command.ClientName);
  }

  private static bool IsRedirectUrisInvalid(CreateClientCommand command)
  {
    if (!command.GrantTypes.Contains(GrantTypeConstants.AuthorizationCode))
      return false;

    return command.RedirectUris is null || !command.RedirectUris.Any() 
           || command.RedirectUris.Any(redirectUri => !Uri.IsWellFormedUriString(redirectUri, UriKind.Absolute));
  }

  private static bool IsResponseTypesInvalid(CreateClientCommand command)
  {
    if (!command.GrantTypes.Contains(GrantTypeConstants.AuthorizationCode))
      return false;

    command.ResponseTypes ??= new List<string>();
    if(!command.ResponseTypes.Any())
      command.ResponseTypes.Add(ResponseTypeConstants.Code);

    return command.ResponseTypes.Any(responseType => !ResponseTypeConstants.ResponseTypes.Contains(responseType));
  }

  private async Task<bool> IsGrantTypeInvalidAsync(CreateClientCommand command)
  {
    if (command.GrantTypes is null || !command.GrantTypes.Any())
      return true;

    var grants = await _identityContext
      .Set<GrantType>()
      .ToListAsync();

    return command.GrantTypes.Any(grantType => grants.All(x => x.Name != grantType));
  }

  private static bool IsContactsInvalid(CreateClientCommand command)
  {
    if (command.Contacts is null || !command.Contacts.Any())
      return false;

    return (
      from contact in command.Contacts
      let ampersandPosition = contact.LastIndexOf('@')
      select ampersandPosition > 0 
             && (contact.LastIndexOf(".", StringComparison.Ordinal) > ampersandPosition) 
             && (contact.Length - ampersandPosition > 4)).Any(isValidContact => !isValidContact);
  }

  private async Task<bool> IsScopeInvalidAsync(CreateClientCommand command)
  {
    if (command.GrantTypes.Contains(GrantTypeConstants.AuthorizationCode) && !command.Scopes.Contains(ScopeConstants.OpenId))
      return true;

    foreach (var scope in command.Scopes)
    {
      if (!await _identityContext.Set<Scope>().AnyAsync(x => x.Name == scope))
        return true;
    }

    return false;
  }

  private static bool IsSubjectTypeInvalid(CreateClientCommand command)
  {
    if (!string.IsNullOrWhiteSpace(command.SubjectType))
      return !SubjectTypeConstants.SubjectTypes.Contains(command.SubjectType);

    command.SubjectType = SubjectTypeConstants.Public;
    return false;
  }

  private static bool IsTokenEndpointAuthMethodInvalid(CreateClientCommand command)
  {
    if (!string.IsNullOrWhiteSpace(command.TokenEndpointAuthMethod))
      return !TokenEndpointAuthMethodConstants.TokenEndpointAuthMethods.Contains(command.TokenEndpointAuthMethod);

    command.SubjectType = TokenEndpointAuthMethodConstants.ClientSecretPost;
    return false;
  }

  private static ValidationResult GetInvalidClientMetadataResult(string errorDescription)
  {
    return new ValidationResult(ErrorCode.InvalidClientMetadata, errorDescription, HttpStatusCode.BadRequest);
  }
}