using Microsoft.Net.Http.Headers;
using System.Text.RegularExpressions;

namespace Specs.Helpers;
public static class AntiForgeryHelper
{
  public static async Task<(string, string)> GetAntiForgeryAsync(HttpResponseMessage response)
  {
    var loginViewHtml = await response.Content.ReadAsStringAsync();

    var antiForgeryCookie = response.Headers
      .GetValues("Set-Cookie")
      .FirstOrDefault(x => x.Contains("AntiForgeryCookie"));

    var antiForgeryCookieValue = SetCookieHeaderValue.Parse(antiForgeryCookie).Value;
    if (string.IsNullOrWhiteSpace(antiForgeryCookieValue.Value))
      throw new Exception("Invalid cookie was provided");

    var antiForgeryFieldMatch = Regex.Match(loginViewHtml, $@"\<input name=""AntiForgeryField"" type=""hidden"" value=""([^""]+)"" \/\>");
    if (!antiForgeryFieldMatch.Captures.Any() && antiForgeryFieldMatch.Groups.Count != 2)
      throw new Exception("Invalid input of antiforgerytoken was provided");

    var antiForgeryField = antiForgeryFieldMatch.Groups[1].Captures[0].Value;

    return (antiForgeryCookieValue.Value, antiForgeryField);
  }
}
