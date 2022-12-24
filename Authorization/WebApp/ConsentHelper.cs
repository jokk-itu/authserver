using WebApp.Constants;

namespace WebApp;
public static class ConsentHelper
{
  public static IEnumerable<string> GetConsentedClaims(IFormCollection formCollection)
  {
    var valuesToIgnore = new[] { AntiForgeryConstants.AntiForgeryField, ParameterNames.LoginCode };
    return formCollection.Keys.Where(x => !valuesToIgnore.Contains(x));
  }
}