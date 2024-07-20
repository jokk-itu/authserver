using System.Net.Http.Json;
using AuthServer.Core;
using AuthServer.Endpoints.Responses;

namespace AuthServer.Tests.IntegrationTest.EndpointBuilders;
public class IntrospectionEndpointBuilder
{
  private string _token = string.Empty;
  private string _tokenTypeHint = string.Empty;
  private string _clientId = string.Empty;
  private string _clientSecret = string.Empty;

  public static IntrospectionEndpointBuilder Instance()
  {
    return new IntrospectionEndpointBuilder();
  }

  public IntrospectionEndpointBuilder AddToken(string token)
  {
    _token = token;
    return this;
  }

  public IntrospectionEndpointBuilder AddTokenTypeHint(string tokenTypeHint)
  {
    _tokenTypeHint = tokenTypeHint;
    return this;
  }

  public IntrospectionEndpointBuilder AddClientId(string clientId)
  {
    _clientId = clientId;
    return this;
  }

  public IntrospectionEndpointBuilder AddClientSecret(string clientSecret)
  {
    _clientSecret = clientSecret;
    return this;
  }

  internal async Task<PostIntrospectionResponse> BuildIntrospection(HttpClient httpClient)
  {
    var tokenContent = new FormUrlEncodedContent(new Dictionary<string, string>
    {
      { Parameter.Token, _token },
      { Parameter.TokenTypeHint, _tokenTypeHint },
      { Parameter.ClientId, _clientId },
      { Parameter.ClientSecret, _clientSecret }
    });

    var refreshTokenRequest = new HttpRequestMessage(HttpMethod.Post, "oauth/introspection")
    {
      Content = tokenContent
    };
    var introspectionResponse = await httpClient.SendAsync(refreshTokenRequest);
    introspectionResponse.EnsureSuccessStatusCode();
    var deserialized = await introspectionResponse.Content.ReadFromJsonAsync<PostIntrospectionResponse>();
    return deserialized!;
  }
}