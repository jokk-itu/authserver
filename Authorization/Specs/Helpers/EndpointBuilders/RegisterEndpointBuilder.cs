using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using WebApp.Constants;
using WebApp.Contracts;

namespace Specs.Helpers.EndpointBuilders;
public class RegisterEndpointBuilder
{
  private string _applicationType = string.Empty;
  private string _tokenEndpointAuthMethod = string.Empty;
  private string _scope = string.Empty;
  private string _clientName = string.Empty;
  private string _policyUri = string.Empty;
  private string _tosUri = string.Empty;
  private string _subjectType = string.Empty;
  private string _defaultMaxAge = string.Empty;
  private string _initiateLoginUri = string.Empty;
  private string _logoUri = string.Empty;
  private string _clientUri = string.Empty;
  private string _backchannelLogoutUri = string.Empty;
  private string _jwks = string.Empty;
  private string _jwksUri = string.Empty;
  private ICollection<string> _grantTypes = new List<string>();
  private ICollection<string> _redirectUris = new List<string>();
  private ICollection<string> _postLogoutRedirectUris = new List<string>();
  private ICollection<string> _responseTypes = new List<string>();
  private ICollection<string> _contacts = new List<string>();

  public static RegisterEndpointBuilder Instance()
  {
    return new RegisterEndpointBuilder();
  }

  public RegisterEndpointBuilder AddApplicationType(string applicationType)
  {
    _applicationType = applicationType;
    return this;
  }

  public RegisterEndpointBuilder AddTokenEndpointAuthMethod(string tokenEndpointAuthMethod)
  {
    _tokenEndpointAuthMethod = tokenEndpointAuthMethod;
    return this;
  }

  public RegisterEndpointBuilder AddScope(string scope)
  {
    _scope = scope;
    return this;
  }

  public RegisterEndpointBuilder AddClientName(string clientName)
  {
    _clientName = clientName;
    return this;
  }

  public RegisterEndpointBuilder AddGrantType(string grantType)
  {
    _grantTypes.Add(grantType);
    return this;
  }

  public RegisterEndpointBuilder AddRedirectUri(string redirectUri)
  {
    _redirectUris.Add(redirectUri);
    return this;
  }

  public RegisterEndpointBuilder AddSubjectType(string subjectType)
  {
    _subjectType = subjectType;
    return this;
  }

  public RegisterEndpointBuilder AddResponseType(string responseType)
  {
    _responseTypes.Add(responseType);
    return this;
  }

  public RegisterEndpointBuilder AddBackchannelLogoutUri(string backchannelLogoutUri)
  {
    _backchannelLogoutUri = backchannelLogoutUri;
    return this;
  }

  public RegisterEndpointBuilder AddPostLogoutRedirectUri(string postLogoutRedirectUri)
  {
    _postLogoutRedirectUris.Add(postLogoutRedirectUri);
    return this;
  }

  public async Task<ClientResponse> BuildClient(HttpClient httpClient, CancellationToken cancellationToken = default)
  {
    var arguments = new Dictionary<string, object>
    {
      { ParameterNames.ApplicationType, _applicationType },
      { ParameterNames.TokenEndpointAuthMethod, _tokenEndpointAuthMethod },
      { ParameterNames.Scope, _scope },
      { ParameterNames.ClientName, _clientName },
      { ParameterNames.PolicyUri, _policyUri },
      { ParameterNames.TosUri, _tosUri },
      { ParameterNames.SubjectType, _subjectType },
      { ParameterNames.DefaultMaxAge, _defaultMaxAge },
      { ParameterNames.InitiateLoginUri, _initiateLoginUri },
      { ParameterNames.LogoUri, _logoUri },
      { ParameterNames.ClientUri, _clientUri },
      { ParameterNames.BackchannelLogoutUri, _backchannelLogoutUri },
      { ParameterNames.Jwks, _jwks },
      { ParameterNames.JwksUri, _jwksUri },
      { ParameterNames.GrantTypes, _grantTypes },
      { ParameterNames.RedirectUris, _redirectUris },
      { ParameterNames.PostLogoutRedirectUris, _postLogoutRedirectUris },
      { ParameterNames.ResponseTypes, _responseTypes },
      { ParameterNames.Contacts, _contacts }
    };
    var request = new HttpRequestMessage(HttpMethod.Post, "connect/register")
    {
      Content = new StringContent(JsonSerializer.Serialize(arguments), Encoding.UTF8, MimeTypeConstants.Json)
    };
    var clientResponse = await httpClient.SendAsync(request, cancellationToken);
    clientResponse.EnsureSuccessStatusCode();
    var deserialized = await clientResponse.Content.ReadFromJsonAsync<ClientResponse>(cancellationToken: cancellationToken);
    return deserialized!;
  }
}