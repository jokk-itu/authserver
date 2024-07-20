using System.Net.Http.Json;
using AuthServer.Core;
using AuthServer.Endpoints.Responses;

namespace AuthServer.Tests.IntegrationTest.EndpointBuilders;
public class TokenEndpointBuilder
{
  private string _clientId = string.Empty;
  private string _clientSecret = string.Empty;
  private string _code = string.Empty;
  private string _grantType = string.Empty;
  private string _redirectUri = string.Empty;
  private string _scope = string.Empty;
  private string _codeVerifier = string.Empty;
  private string _refreshToken = string.Empty;
  private readonly ICollection<string> _resource = [];

  public static TokenEndpointBuilder Instance()
  {
    return new TokenEndpointBuilder();
  }

  public TokenEndpointBuilder AddClientId(string clientId)
  {
    _clientId = clientId;
    return this;
  }

  public TokenEndpointBuilder AddClientSecret(string clientSecret)
  {
    _clientSecret = clientSecret;
    return this;
  }

  public TokenEndpointBuilder AddCode(string code)
  {
    _code = code;
    return this;
  }

  public TokenEndpointBuilder AddGrantType(string grantType)
  {
    _grantType = grantType;
    return this;
  }

  public TokenEndpointBuilder AddRedirectUri(string redirectUri)
  {
    _redirectUri = redirectUri;
    return this;
  }

  public TokenEndpointBuilder AddScope(string scope)
  {
    _scope = scope;
    return this;
  }

  public TokenEndpointBuilder AddCodeVerifier(string codeVerifier)
  {
    _codeVerifier = codeVerifier;
    return this;
  }

  public TokenEndpointBuilder AddRefreshToken(string refreshToken)
  {
    _refreshToken = refreshToken;
    return this;
  }

  public TokenEndpointBuilder AddResource(string resource)
  {
    _resource.Add(resource);
    return this;
  }

  internal async Task<PostTokenResponse> BuildRedeemAuthorizationCode(HttpClient httpClient, CancellationToken cancellationToken = default)
  {
    var body = new List<KeyValuePair<string, string>>
    {
      new(Parameter.ClientId, _clientId),
      new(Parameter.ClientSecret, _clientSecret),
      new(Parameter.Code, _code),
      new(Parameter.GrantType, _grantType),
      new(Parameter.RedirectUri, _redirectUri),
      new(Parameter.Scope, _scope),
      new(Parameter.CodeVerifier, _codeVerifier)
    };
    body.AddRange(_resource.Select(r => new KeyValuePair<string, string>(Parameter.Resource, r)));

    var tokenContent = new FormUrlEncodedContent(body);
    var request = new HttpRequestMessage(HttpMethod.Post, "connect/token")
    {
      Content = tokenContent
    };
    var tokenResponse = await httpClient.SendAsync(request, cancellationToken);
    tokenResponse.EnsureSuccessStatusCode();
    var deserialized =  await tokenResponse.Content.ReadFromJsonAsync<PostTokenResponse>(cancellationToken: cancellationToken);
    return deserialized!;
  }

  internal async Task<PostTokenResponse> BuildRedeemRefreshToken(HttpClient httpClient,
    CancellationToken cancellationToken = default)
  {
    var body = new List<KeyValuePair<string, string>>
    {
      new(Parameter.ClientId, _clientId),
      new(Parameter.ClientSecret, _clientSecret),
      new(Parameter.GrantType, _grantType),
      new(Parameter.RefreshToken, _refreshToken),
      new(Parameter.Scope, _scope)
    };
    body.AddRange(_resource.Select(r => new KeyValuePair<string, string>(Parameter.Resource, r)));

    var tokenContent = new FormUrlEncodedContent(body);
    var refreshTokenRequest = new HttpRequestMessage(HttpMethod.Post, "connect/token")
    {
      Content = tokenContent
    };
    var tokenResponse = await httpClient.SendAsync(refreshTokenRequest, cancellationToken);
    tokenResponse.EnsureSuccessStatusCode();
    var deserialized = await tokenResponse.Content.ReadFromJsonAsync<PostTokenResponse>(cancellationToken: cancellationToken);
    return deserialized!;
  }

  internal async Task<PostTokenResponse> BuildRedeemClientCredentials(HttpClient httpClient,
    CancellationToken cancellationToken = default)
  {
    var body = new List<KeyValuePair<string, string>>
    {
      new(Parameter.ClientId, _clientId),
      new(Parameter.ClientSecret, _clientSecret),
      new(Parameter.GrantType, _grantType),
      new(Parameter.Scope, _scope)
    };
    body.AddRange(_resource.Select(r => new KeyValuePair<string, string>(Parameter.Resource, r)));

    var tokenContent = new FormUrlEncodedContent(body);
    var request = new HttpRequestMessage(HttpMethod.Post, "connect/token")
    {
      Content = tokenContent
    };
    var tokenResponse = await httpClient.SendAsync(request, cancellationToken);
    tokenResponse.EnsureSuccessStatusCode();
    var deserialized = await tokenResponse.Content.ReadFromJsonAsync<PostTokenResponse>(cancellationToken: cancellationToken);
    return deserialized!;
  }
}