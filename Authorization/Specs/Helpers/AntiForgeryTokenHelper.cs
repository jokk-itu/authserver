using Microsoft.Net.Http.Headers;
using System.Text.RegularExpressions;
using WebApp.Constants;

namespace Specs.Helpers;
public class AntiForgeryTokenHelper
{
  private string _cookie = string.Empty;
  private readonly HttpClient _httpClient;

  public AntiForgeryTokenHelper(HttpClient httpClient)
  {
    _httpClient = httpClient;
  }

  public async Task<AntiForgeryToken> GetAntiForgeryToken(string path, string authenticationCookie)
  {
    var request = new HttpRequestMessage(HttpMethod.Get, path);
    request.Headers.Add("Cookie", authenticationCookie);
    var response = await _httpClient.SendAsync(request);
    return await GetAntiForgeryTokenInternal(response);
  }

  public async Task<AntiForgeryToken> GetAntiForgeryToken(string path)
  {
    var response = await _httpClient.GetAsync(path);
    return await GetAntiForgeryTokenInternal(response);
  }

  private async Task<AntiForgeryToken> GetAntiForgeryTokenInternal(HttpResponseMessage response)
  {
    var html = await response.Content.ReadAsStringAsync();

    if (string.IsNullOrWhiteSpace(_cookie))
    {
      var antiForgeryCookie = response.Headers
        .GetValues("Set-Cookie")
        .FirstOrDefault(x => x.Contains(AntiForgeryConstants.AntiForgeryCookie));

      var antiForgeryCookieValue = SetCookieHeaderValue.Parse(antiForgeryCookie).Value;
      if (string.IsNullOrWhiteSpace(antiForgeryCookieValue.Value))
      {
        throw new Exception("Invalid cookie was provided");
      }

      _cookie = antiForgeryCookieValue.Value;
    }

    var antiForgeryFieldMatch = Regex.Match(html, $@"\<input name=""{AntiForgeryConstants.AntiForgeryField}"" type=""hidden"" value=""([^""]+)"" \/\>");
    if (!antiForgeryFieldMatch.Captures.Any() && antiForgeryFieldMatch.Groups.Count != 2)
    {
      throw new Exception($"Invalid input of anti-forgery-token was provided{Environment.NewLine}{html}");
    }

    var antiForgeryField = antiForgeryFieldMatch.Groups[1].Captures[0].Value;

    return new AntiForgeryToken
    {
      Cookie = _cookie,
      Field = antiForgeryField
    };
  }
}
