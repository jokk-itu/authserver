using System.Net;
using Application;
using Application.Validation;
using Domain;
using Domain.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Requests.CreateClient;
public class CreateClientValidator : IValidator<CreateClientCommand>
{
  private readonly IdentityContext _identityContext;
  private readonly HttpClient _httpClient;

  public CreateClientValidator(
    IdentityContext identityContext,
    IHttpClientFactory httpClientFactory)
  {
    _identityContext = identityContext;
    _httpClient = httpClientFactory.CreateClient();
  }

  public async Task<ValidationResult> ValidateAsync(CreateClientCommand value, CancellationToken cancellationToken = default)
  {
    if (IsApplicationTypeInvalid(value))
    {
      return GetInvalidClientMetadataResult("application_type is invalid");
    }

    if (await IsClientNameInvalidAsync(value))
    {
      return GetInvalidClientMetadataResult("client_name is required");
    }

    if (IsRedirectUrisInvalid(value))
    {
      return GetInvalidClientMetadataResult("redirect_uri is invalid");
    }

    if (IsPostLogoutRedirectUrisInvalid(value))
    {
      return GetInvalidClientMetadataResult("post_logout_redirect_uris is invalid");
    }

    if (IsResponseTypesInvalid(value))
    {
      return GetInvalidClientMetadataResult("response_type is invalid");
    }

    if (await IsGrantTypeInvalidAsync(value))
    {
      return GetInvalidClientMetadataResult("grant_type is invalid");
    }

    if (IsContactsInvalid(value))
    {
      return GetInvalidClientMetadataResult("contacts is invalid");
    }

    if (await IsScopeInvalidAsync(value))
    {
      return GetInvalidClientMetadataResult("scope is invalid");
    }

    if (!string.IsNullOrWhiteSpace(value.PolicyUri)
        && !Uri.IsWellFormedUriString(value.PolicyUri, UriKind.Absolute))
    {
      return GetInvalidClientMetadataResult("policy_uri must be a well-formed uri");
    }

    if (!string.IsNullOrWhiteSpace(value.TosUri)
        && !Uri.IsWellFormedUriString(value.TosUri, UriKind.Absolute))
    {
      return GetInvalidClientMetadataResult("tos_uri must be a well-formed uri");
    }
    
    if (!string.IsNullOrWhiteSpace(value.InitiateLoginUri)
        && !Uri.IsWellFormedUriString(value.InitiateLoginUri, UriKind.Absolute))
    {
      return GetInvalidClientMetadataResult("initiate_logo_uri must be a well-formed uri");
    }

    if (!string.IsNullOrWhiteSpace(value.LogoUri)
        && !Uri.IsWellFormedUriString(value.LogoUri, UriKind.Absolute))
    {
      return GetInvalidClientMetadataResult("logo_uri must be a well-formed uri");
    }

    if (!string.IsNullOrWhiteSpace(value.ClientUri)
        && !Uri.IsWellFormedUriString(value.ClientUri, UriKind.Absolute))
    {
      return GetInvalidClientMetadataResult("client_uri must be a well-formed uri");
    }

    if (IsSubjectTypeInvalid(value))
    {
      return GetInvalidClientMetadataResult("subject_type is invalid");
    }

    if (IsTokenEndpointAuthMethodInvalid(value))
    {
      return GetInvalidClientMetadataResult("token_endpoint_auth_method is invalid");
    }

    if (IsDefaultMaxAgeInvalid(value))
    {
      return GetInvalidClientMetadataResult("default_max_age is invalid");
    }

    if (IsBackChannelLogoutUriInvalid(value))
    {
      return GetInvalidClientMetadataResult("backchannel_logout_uri is invalid");
    }

    if (!string.IsNullOrWhiteSpace(value.Jwks) && !string.IsNullOrWhiteSpace(value.JwksUri))
    {
      return GetInvalidClientMetadataResult("jwks and jwks_uri must not be used together");
    }

    if (IsJwksInvalid(value))
    {
      return GetInvalidClientMetadataResult("jwks is invalid");
    }

    if (await IsJwksUriInvalid(value, cancellationToken))
    {
      return GetInvalidClientMetadataResult("jwks_uri is invalid");
    }

    return new ValidationResult(HttpStatusCode.OK);
  }

  private async Task<bool> IsJwksUriInvalid(CreateClientCommand command, CancellationToken cancellationToken)
  {
    if (string.IsNullOrWhiteSpace(command.JwksUri))
    {
      return false;
    }

    if (!Uri.TryCreate(command.JwksUri, UriKind.Absolute, out var uri))
    {
      return true;
    }

    try
    {
      var request = new HttpRequestMessage(HttpMethod.Get, uri);
      var response = await _httpClient.SendAsync(request, cancellationToken);
      response.EnsureSuccessStatusCode();
      var jwks = await response.Content.ReadAsStringAsync(cancellationToken: cancellationToken); 
      var temp = JsonWebKeySet.Create(jwks);
      var isValid = temp.GetSigningKeys().Any() && temp.GetSigningKeys().Count == temp.Keys.Count;
      if (isValid)
      {
        command.Jwks = jwks;
      }

      return !isValid;
    }
    catch (Exception)
    {
      return true;
    }
  }

  private static bool IsJwksInvalid(CreateClientCommand command)
  {
    if (string.IsNullOrWhiteSpace(command.Jwks))
    {
      return false;
    }
    
    var jwks = JsonWebKeySet.Create(command.Jwks);
    var isValid = jwks.GetSigningKeys().Any() && jwks.GetSigningKeys().Count == jwks.Keys.Count;
    return !isValid;
  }

  private static bool IsBackChannelLogoutUriInvalid(CreateClientCommand command)
  {
    if(string.IsNullOrWhiteSpace(command.BackchannelLogoutUri))
    {
      return false;
    }

    return !(Uri.TryCreate(command.BackchannelLogoutUri, UriKind.Absolute, out var uri)
             && string.IsNullOrWhiteSpace(uri.Fragment));
  }

  private static bool IsApplicationTypeInvalid(CreateClientCommand command)
  {
    if (string.IsNullOrWhiteSpace(command.ApplicationType))
    {
      command.ApplicationType = ApplicationTypeConstants.Web;
    }

    return !ApplicationTypeConstants.ApplicationTypes.Contains(command.ApplicationType);
  }

  private async Task<bool> IsClientNameInvalidAsync(CreateClientCommand command)
  {
    if (string.IsNullOrWhiteSpace(command.ClientName))
    {
      return true;
    }

    return await _identityContext
      .Set<Client>()
      .AnyAsync(x => x.Name == command.ClientName);
  }

  private static bool IsRedirectUrisInvalid(CreateClientCommand command)
  {
    if (!command.GrantTypes.Contains(GrantTypeConstants.AuthorizationCode))
    {
      return false;
    }

    return !command.RedirectUris.Any()
           || command.RedirectUris.Any(redirectUri => !Uri.IsWellFormedUriString(redirectUri, UriKind.Absolute));
  }

  private static bool IsPostLogoutRedirectUrisInvalid(CreateClientCommand command)
  {
    return command.PostLogoutRedirectUris.Any(redirectUri => !Uri.IsWellFormedUriString(redirectUri, UriKind.Absolute));
  }

  private static bool IsResponseTypesInvalid(CreateClientCommand command)
  {
    if (!command.GrantTypes.Contains(GrantTypeConstants.AuthorizationCode))
    {
      return false;
    }

    if (!command.ResponseTypes.Any())
    {
      command.ResponseTypes.Add(ResponseTypeConstants.Code);
    }

    return command.ResponseTypes.Any(responseType => !ResponseTypeConstants.ResponseTypes.Contains(responseType));
  }

  private async Task<bool> IsGrantTypeInvalidAsync(CreateClientCommand command)
  {
    if (!command.GrantTypes.Any())
    {
      return true;
    }

    var grants = await _identityContext
      .Set<GrantType>()
      .ToListAsync();

    return command.GrantTypes.Any(grantType => grants.All(x => x.Name != grantType));
  }

  private static bool IsContactsInvalid(CreateClientCommand command)
  {
    if (!command.Contacts.Any())
    {
      return false;
    }

    return (
      from contact in command.Contacts
      let ampersandPosition = contact.LastIndexOf('@')
      select ampersandPosition > 0 
             && (contact.LastIndexOf(".", StringComparison.Ordinal) > ampersandPosition) 
             && (contact.Length - ampersandPosition > 4)).Any(isValidContact => !isValidContact);
  }

  private async Task<bool> IsScopeInvalidAsync(CreateClientCommand command)
  {
    var scopes = new List<string>();
    if (!string.IsNullOrWhiteSpace(command.Scope))
    {
      scopes = command.Scope.Split(' ').ToList();
    }
    else if(command.GrantTypes.Contains(GrantTypeConstants.AuthorizationCode))
    {
      scopes.Add(ScopeConstants.OpenId);
      scopes.Add(ScopeConstants.UserInfo);
    }

    var matchingScopes = await _identityContext
      .Set<Scope>()
      .Where(s => scopes.Contains(s.Name))
      .CountAsync();

    return matchingScopes != scopes.Count;
  }

  private static bool IsSubjectTypeInvalid(CreateClientCommand command)
  {
    if (!string.IsNullOrWhiteSpace(command.SubjectType))
    {
      return !SubjectTypeConstants.SubjectTypes.Contains(command.SubjectType);
    }

    if (command.GrantTypes.Contains(GrantTypeConstants.AuthorizationCode))
    {
      command.SubjectType = SubjectTypeConstants.Public;
    }
    
    return false;
  }

  private static bool IsTokenEndpointAuthMethodInvalid(CreateClientCommand command)
  {
    if (!string.IsNullOrWhiteSpace(command.TokenEndpointAuthMethod))
    {
      return !TokenEndpointAuthMethodConstants.AuthMethods.Contains(command.TokenEndpointAuthMethod);
    }

    command.TokenEndpointAuthMethod = TokenEndpointAuthMethodConstants.ClientSecretBasic;
    return false;
  }

  private static bool IsDefaultMaxAgeInvalid(CreateClientCommand command)
  {
    if (string.IsNullOrWhiteSpace(command.DefaultMaxAge))
    {
      return false;
    }

    if (!long.TryParse(command.DefaultMaxAge, out var defaultMaxAge))
    {
      return true;
    }

    return defaultMaxAge < 0;
  }

  private static ValidationResult GetInvalidClientMetadataResult(string errorDescription)
  {
    return new ValidationResult(ErrorCode.InvalidClientMetadata, errorDescription, HttpStatusCode.BadRequest);
  }
}