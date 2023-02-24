using System.Net.Http.Json;
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

  public async Task<PostTokenResponse> BuildRedeemAuthorizationCode(HttpClient httpClient, CancellationToken cancellationToken = default)
  {
    
    var tokenContent = new FormUrlEncodedContent(new Dictionary<string, string>
    {
      { ParameterNames.ClientId, _clientId },
      { ParameterNames.ClientSecret, _clientSecret },
      { ParameterNames.Code, _code },
      { ParameterNames.GrantType, _grantType },
      { ParameterNames.RedirectUri, _redirectUri },
      { ParameterNames.Scope, _scope },
      { ParameterNames.CodeVerifier, _codeVerifier }
    });
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
      { ParameterNames.RefreshToken, _refreshToken }
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
}