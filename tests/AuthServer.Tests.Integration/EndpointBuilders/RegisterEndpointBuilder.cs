using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using AuthServer.Constants;
using AuthServer.Core;
using AuthServer.Endpoints.Responses;

namespace AuthServer.Tests.Integration.EndpointBuilders;
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
  private readonly ICollection<string> _grantTypes = [];
  private readonly ICollection<string> _redirectUris = [];
  private readonly ICollection<string> _postLogoutRedirectUris = [];
  private readonly ICollection<string> _responseTypes = [];
  private readonly ICollection<string> _contacts = [];

  public static RegisterEndpointBuilder Instance()
  {
    return new RegisterEndpointBuilder();
  }

  public RegisterEndpointBuilder AddClientUri(string clientUri)
  {
    _clientUri = clientUri;
    return this;
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

  internal async Task<PostRegisterResponse> BuildClient(HttpClient httpClient, CancellationToken cancellationToken = default)
  {
    var arguments = new Dictionary<string, object>
    {
      { Parameter.ApplicationType, _applicationType },
      { Parameter.TokenEndpointAuthMethod, _tokenEndpointAuthMethod },
      { Parameter.Scope, _scope },
      { Parameter.ClientName, _clientName },
      { Parameter.PolicyUri, _policyUri },
      { Parameter.TosUri, _tosUri },
      { Parameter.SubjectType, _subjectType },
      { Parameter.DefaultMaxAge, _defaultMaxAge },
      { Parameter.InitiateLoginUri, _initiateLoginUri },
      { Parameter.LogoUri, _logoUri },
      { Parameter.ClientUri, _clientUri },
      { Parameter.BackchannelLogoutUri, _backchannelLogoutUri },
      { Parameter.Jwks, _jwks },
      { Parameter.JwksUri, _jwksUri },
      { Parameter.GrantTypes, _grantTypes },
      { Parameter.RedirectUris, _redirectUris },
      { Parameter.PostLogoutRedirectUris, _postLogoutRedirectUris },
      { Parameter.ResponseTypes, _responseTypes },
      { Parameter.Contacts, _contacts }
    };
    var request = new HttpRequestMessage(HttpMethod.Post, "connect/register")
    {
      Content = new StringContent(JsonSerializer.Serialize(arguments), Encoding.UTF8, MimeTypeConstants.Json)
    };
    var clientResponse = await httpClient.SendAsync(request, cancellationToken);
    clientResponse.EnsureSuccessStatusCode();
    var deserialized = await clientResponse.Content.ReadFromJsonAsync<PostRegisterResponse>(cancellationToken: cancellationToken);
    return deserialized!;
  }
}