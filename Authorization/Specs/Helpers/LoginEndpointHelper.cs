using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using WebApp.Constants;

namespace Specs.Helpers;
public static class LoginEndpointHelper
{
  public static async Task<HttpResponseMessage> GetLoginCode(HttpClient client, QueryString query, string username, string password, AntiForgeryToken antiForgeryToken)
  {
    var postLoginRequest = new HttpRequestMessage(HttpMethod.Post, $"connect/login{query}");
    postLoginRequest.Headers.Add("Cookie", new CookieHeaderValue("AntiForgeryCookie", antiForgeryToken.Cookie).ToString());
    var loginForm = new FormUrlEncodedContent(new Dictionary<string, string>
    {
      { ParameterNames.Username, username },
      { ParameterNames.Password, password },
      { "AntiForgeryField", antiForgeryToken.Field }
    });
    postLoginRequest.Content = loginForm; 
    return await client.SendAsync(postLoginRequest);
  }
}
