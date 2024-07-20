using System.Net;
using AuthServer.Core;

namespace AuthServer.Tests.IntegrationTest.EndpointBuilders;
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
      { Parameter.Token, _token },
      { Parameter.TokenTypeHint, _tokenTypeHint },
      { Parameter.ClientId, _clientId },
      { Parameter.ClientSecret, _clientSecret }
    });

    var refreshTokenRequest = new HttpRequestMessage(HttpMethod.Post, "oauth/revoke")
    {
      Content = tokenContent
    };
    var introspectionResponse = await httpClient.SendAsync(refreshTokenRequest);
    introspectionResponse.EnsureSuccessStatusCode();
    return introspectionResponse.StatusCode;
  }
}