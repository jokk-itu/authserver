using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;

namespace Specs.Helpers;
public static class AuthorizeEndpointHelper
{
  public static async Task<HttpResponseMessage> GetAuthorizationCodeAsync(HttpClient client, QueryString query, string username, string password)
  {
    var forgeryToken = await AntiForgeryHelper.GetAntiForgeryTokenAsync(client, $"connect/v1/authorize/{query}");
    var postAuthorizeRequest = new HttpRequestMessage(HttpMethod.Post, $"connect/v1/authorize{query}");
    postAuthorizeRequest.Headers.Add("Cookie", new CookieHeaderValue("AntiForgeryCookie", forgeryToken.Cookie).ToString());
    var loginForm = new FormUrlEncodedContent(new Dictionary<string, string>
    {
      { "username", username },
      { "password", password },
      { "AntiForgeryField", forgeryToken.Field }
    });
    postAuthorizeRequest.Content = loginForm; 
    return await client.SendAsync(postAuthorizeRequest);
  }
}
