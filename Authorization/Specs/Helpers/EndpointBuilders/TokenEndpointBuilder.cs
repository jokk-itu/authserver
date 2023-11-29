using System.Collections.Specialized;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Server.IIS.Core;
using WebApp.Constants;
using WebApp.Contracts.PostToken;

namespace Specs.Helpers.EndpointBuilders;
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
  private ICollection<string> _resource = new List<string>();

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

  public async Task<PostTokenResponse> BuildRedeemAuthorizationCode(HttpClient httpClient, CancellationToken cancellationToken = default)
  {
    var tokenContent = new FormUrlEncodedContent(new Dictionary<string, string>
    {
      new(ParameterNames.ClientId, _clientId),
      new(ParameterNames.ClientSecret, _clientSecret),
      new(ParameterNames.Code, _code),
      new(ParameterNames.GrantType, _grantType),
      new(ParameterNames.RedirectUri, _redirectUri),
      new(ParameterNames.Scope, _scope),
      new(ParameterNames.CodeVerifier, _codeVerifier)
    };
    foreach (var r in _resource)
    {
      body.Add(new KeyValuePair<string, string>(ParameterNames.Resource, r));
    }

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

  public async Task<PostTokenResponse> BuildRedeemRefreshToken(HttpClient httpClient,
    CancellationToken cancellationToken = default)
  {
    var tokenContent = new FormUrlEncodedContent(new Dictionary<string, string>
    {
      { ParameterNames.ClientId, _clientId },
      { ParameterNames.ClientSecret, _clientSecret },
      { ParameterNames.GrantType, _grantType },
      { ParameterNames.RefreshToken, _refreshToken },
      { ParameterNames.Scope, _scope }
    });

    var refreshTokenRequest = new HttpRequestMessage(HttpMethod.Post, "connect/token")
    {
      Content = tokenContent
    };
    var tokenResponse = await httpClient.SendAsync(refreshTokenRequest, cancellationToken);
    tokenResponse.EnsureSuccessStatusCode();
    var deserialized = await tokenResponse.Content.ReadFromJsonAsync<PostTokenResponse>(cancellationToken: cancellationToken);
    return deserialized!;
  }

  public async Task<PostTokenResponse> BuildRedeemClientCredentials(HttpClient httpClient,
    CancellationToken cancellationToken = default)
  {
    var tokenContent = new FormUrlEncodedContent(new Dictionary<string, string>
    {
      { ParameterNames.ClientId, _clientId },
      { ParameterNames.ClientSecret, _clientSecret },
      { ParameterNames.GrantType, OpenIdConnectGrantTypes.ClientCredentials },
      { ParameterNames.Scope, _scope }
    });
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