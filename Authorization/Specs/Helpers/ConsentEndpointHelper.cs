using Domain.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using WebApp.Constants;

namespace Specs.Helpers;
public static class ConsentEndpointHelper
{
  public static async Task<HttpResponseMessage> GetConsent(HttpClient client, QueryString query, AntiForgeryToken antiForgeryToken)
  {
    var postConsentRequest = new HttpRequestMessage(HttpMethod.Post, $"connect/consent{query}");
    postConsentRequest.Headers.Add("Cookie", new CookieHeaderValue(AntiForgeryConstants.AntiForgeryCookie, antiForgeryToken.Cookie).ToString());
    var consentForm = new FormUrlEncodedContent(new Dictionary<string, string>
    {
      { ClaimNameConstants.Name, "on" },
      { ClaimNameConstants.Address, "on" },
      { AntiForgeryConstants.AntiForgeryField, antiForgeryToken.Field }
    });
    postConsentRequest.Content = consentForm; 
    return await client.SendAsync(postConsentRequest);
  }
}