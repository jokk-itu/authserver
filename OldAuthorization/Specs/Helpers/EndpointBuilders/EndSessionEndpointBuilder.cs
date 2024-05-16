using System.Net;
using Microsoft.AspNetCore.Http.Extensions;
using WebApp.Constants;

namespace Specs.Helpers.EndpointBuilders;
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
      { ParameterNames.IdTokenHint, _idTokenHint },
      { ParameterNames.PostLogoutRedirectUri, _postLogoutRedirectUri },
      { ParameterNames.ClientId, _clientId },
      { ParameterNames.State, _state }
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
      { ParameterNames.IdTokenHint, _idTokenHint },
      { ParameterNames.ClientId, _clientId },
      { ParameterNames.PostLogoutRedirectUri, _postLogoutRedirectUri },
      { ParameterNames.State, _state }
    }.ToQueryString();
    var endSessionResponse = await httpClient.GetAsync($"connect/end-session{query}");
    return endSessionResponse.StatusCode;
  }
}