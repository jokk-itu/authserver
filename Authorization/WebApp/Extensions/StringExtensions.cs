using Microsoft.Extensions.Primitives;
using System.Web;

namespace WebApp.Extensions;

public static class StringExtensions
{
  public static string DecodeFromFormUrl(this StringValues value)
  {
    var urlDecoded = HttpUtility.UrlDecode(value);
    var formDecoded = HttpUtility.ParseQueryString(urlDecoded);
    return formDecoded.ToString()!.Replace("%2f", "/");
  }
}
