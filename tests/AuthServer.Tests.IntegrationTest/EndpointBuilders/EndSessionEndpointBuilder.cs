using System.Net;
using AuthServer.Core;
using Microsoft.AspNetCore.Http.Extensions;

namespace AuthServer.Tests.IntegrationTest.EndpointBuilders;
public class EndSessionEndpointBuilder
{
  private string _state = string.Empty;
  private string _clientId = string.Empty;
  private string _postLogoutRedirectUri = string.Empty;
  private string _idTokenHint = string.Empty;

  public static EndSessionEndpointBuilder Instance()
  {
    return new EndSessionEndpointBuilder();
  }

  public EndSessionEndpointBuilder AddState(string state)
  {
    _state = state;
    return this;
  }

  public EndSessionEndpointBuilder AddClientId(string clientId)
  {
    _clientId = clientId;
    return this;
  }

  public EndSessionEndpointBuilder AddPostLogoutRedirectUri(string postLogoutRedirectUr)
  {
    _postLogoutRedirectUri = postLogoutRedirectUr;
    return this;
  }

  public EndSessionEndpointBuilder AddIdTokenHint(string idTokenHint)
  {
    _idTokenHint = idTokenHint;
    return this;
  }

  public async Task<HttpStatusCode> BuildEndSessionAsPost(HttpClient httpClient)
  {
    var tokenContent = new FormUrlEncodedContent(new Dictionary<string, string>
    {
      { Parameter.IdTokenHint, _idTokenHint },
      { Parameter.PostLogoutRedirectUri, _postLogoutRedirectUri },
      { Parameter.ClientId, _clientId },
      { Parameter.State, _state }
    });

    var refreshTokenRequest = new HttpRequestMessage(HttpMethod.Post, "connect/end-session")
    {
      Content = tokenContent
    };
    var endSessionResponse = await httpClient.SendAsync(refreshTokenRequest);
    return endSessionResponse.StatusCode;
  }

  public async Task<HttpStatusCode> BuildEndSessionAsQuery(HttpClient httpClient)
  {
    var query = new QueryBuilder
    {
      { Parameter.IdTokenHint, _idTokenHint },
      { Parameter.ClientId, _clientId },
      { Parameter.PostLogoutRedirectUri, _postLogoutRedirectUri },
      { Parameter.State, _state }
    }.ToQueryString();
    var endSessionResponse = await httpClient.GetAsync($"connect/end-session{query}");
    return endSessionResponse.StatusCode;
  }
}