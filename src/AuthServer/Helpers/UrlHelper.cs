namespace AuthServer.Helpers;
internal static class UrlHelper
{
    private static Uri? GetAbsoluteUrl(string url)
        => Uri.TryCreate(url, UriKind.Absolute, out var parsedUrl) ? parsedUrl : null;

    /// <summary>
    /// The URL is valid if
    /// * It does not contain a query or fragment part
    /// * The scheme is https or private or the host is loopback
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    public static bool IsUrlValidForNativeClient(string url)
    {
        var parsedUrl = GetAbsoluteUrl(url);
        if (parsedUrl is null)
        {
            return false;
        }

        var hasQuery = !string.IsNullOrEmpty(parsedUrl.Query);
        var hasFragment = !string.IsNullOrEmpty(parsedUrl.Fragment);
        var isHttps = parsedUrl.Scheme == Uri.UriSchemeHttps;
        var isLoopback = parsedUrl.IsLoopback;
        var isPrivateScheme = parsedUrl.Scheme.Split('.').Length > 1;
        return !hasQuery && !hasFragment && (isHttps || isLoopback || isPrivateScheme);
    }

    /// <summary>
    /// The URL is valid if
    /// * It is absolute
    /// * The host is not loopback
    /// * The scheme is https
    /// * It does not contain a query or fragment part
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    public static bool IsUrlValidForWebClient(string url)
    {
        var parsedUrl = GetAbsoluteUrl(url);
        if (parsedUrl is null)
        {
            return false;
        }

        var hasQuery = !string.IsNullOrEmpty(parsedUrl.Query);
        var hasFragment = !string.IsNullOrEmpty(parsedUrl.Fragment);
        var isHttps = parsedUrl.Scheme == Uri.UriSchemeHttps;
        var isLoopback = parsedUrl.IsLoopback;
        return !hasQuery && !hasFragment && isHttps && !isLoopback;
    }
}
