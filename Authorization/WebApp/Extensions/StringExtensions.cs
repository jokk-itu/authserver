using System.Web;
using System.Linq;
using Microsoft.Extensions.Primitives;

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
