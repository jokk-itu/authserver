using System.Net;
using WebApp.Constants;

namespace Specs.Helpers.EndpointBuilders;
public class RevocationEndpointBuilder
{
  private string _token = string.Empty;
  private string _tokenTypeHint = string.Empty;
  private string _clientId = string.Empty;
  private string _clientSecret = string.Empty;

  public static RevocationEndpointBuilder Instance()
  {
    return new RevocationEndpointBuilder();
  }

  public RevocationEndpointBuilder AddToken(string token)
  {
    _token = token;
    return this;
  }

  public RevocationEndpointBuilder AddTokenTypeHint(string tokenTypeHint)
  {
    _tokenTypeHint = tokenTypeHint;
    return this;
  }

  public RevocationEndpointBuilder AddClientId(string clientId)
  {
    _clientId = clientId;
    return this;
  }

  public RevocationEndpointBuilder AddClientSecret(string clientSecret)
  {
    _clientSecret = clientSecret;
    return this;
  }

  public async Task<HttpStatusCode> BuildRevocation(HttpClient httpClient)
  {
    var tokenContent = new FormUrlEncodedContent(new Dictionary<string, string>
    {
      { ParameterNames.Token, _token },
      { ParameterNames.TokenTypeHint, _tokenTypeHint },
      { ParameterNames.ClientId, _clientId },
      { ParameterNames.ClientSecret, _clientSecret }
    });

    var refreshTokenRequest = new HttpRequestMessage(HttpMethod.Post, "connect/token/revocation")
    {
      Content = tokenContent
    };
    var introspectionResponse = await httpClient.SendAsync(refreshTokenRequest);
    introspectionResponse.EnsureSuccessStatusCode();
    return introspectionResponse.StatusCode;
  }
}