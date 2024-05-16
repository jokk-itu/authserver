using System.Net;
using WebApp.Constants;

namespace Specs.Helpers.EndpointBuilders;
public class RevokeEndpointBuilder
{
  private string _token = string.Empty;
  private string _tokenTypeHint = string.Empty;
  private string _clientId = string.Empty;
  private string _clientSecret = string.Empty;

  public static RevokeEndpointBuilder Instance()
  {
    return new RevokeEndpointBuilder();
  }

  public RevokeEndpointBuilder AddToken(string token)
  {
    _token = token;
    return this;
  }

  public RevokeEndpointBuilder AddTokenTypeHint(string tokenTypeHint)
  {
    _tokenTypeHint = tokenTypeHint;
    return this;
  }

  public RevokeEndpointBuilder AddClientId(string clientId)
  {
    _clientId = clientId;
    return this;
  }

  public RevokeEndpointBuilder AddClientSecret(string clientSecret)
  {
    _clientSecret = clientSecret;
    return this;
  }

  public async Task<HttpStatusCode> BuildRevoke(HttpClient httpClient)
  {
    var tokenContent = new FormUrlEncodedContent(new Dictionary<string, string>
    {
      { ParameterNames.Token, _token },
      { ParameterNames.TokenTypeHint, _tokenTypeHint },
      { ParameterNames.ClientId, _clientId },
      { ParameterNames.ClientSecret, _clientSecret }
    });

    var refreshTokenRequest = new HttpRequestMessage(HttpMethod.Post, "connect/revoke")
    {
      Content = tokenContent
    };
    var introspectionResponse = await httpClient.SendAsync(refreshTokenRequest);
    introspectionResponse.EnsureSuccessStatusCode();
    return introspectionResponse.StatusCode;
  }
}