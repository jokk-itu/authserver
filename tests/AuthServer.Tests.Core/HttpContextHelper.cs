using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;

namespace AuthServer.Tests.Core;

public static class HttpContextHelper
{
    public static IRequestCookieCollection GetRequestCookies(IDictionary<string, string> cookies)
    {
        var requestFeature = new HttpRequestFeature();
        var featureCollection = new FeatureCollection();

        requestFeature.Headers = new HeaderDictionary();
        foreach (var cookie in cookies)
        {
            requestFeature.Headers.Append(HeaderNames.Cookie, new StringValues($"{cookie.Key}={cookie.Value}"));
        }

        featureCollection.Set<IHttpRequestFeature>(requestFeature);

        var cookiesFeature = new RequestCookiesFeature(featureCollection);

        return cookiesFeature.Cookies;
    }
}