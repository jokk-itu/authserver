using System.Text;
using WebApp.Constants;

namespace WebApp;
public static class FormPostBuilder
{
  public static string BuildAuthorizationCodeResponse(string redirectUri, string state, string code, string iss)
  {
    var formBuilder = new StringBuilder();
    formBuilder.Append(AddStart(redirectUri));
    formBuilder.Append(AddInput(ParameterNames.State, state));
    formBuilder.Append(AddInput(ParameterNames.Code, code));
    formBuilder.Append(AddInput(ParameterNames.Issuer, iss));
    formBuilder.Append(AddEnd());
    return formBuilder.ToString();
  }

  private static string AddStart(string redirectUri)
  {
    return $@"
<!DOCTYPE html>
<html>
<head>
<title>Submit</title>
</head>
<body onload=""javascript:document.forms[0].submit()"">
<p>Redirecting to client</p>
<form method=""post"" action=""{redirectUri}"">
";
  }

  private static string AddEnd()
  {
    return @"
</form>
</body>
</html>";
  }

  private static string AddInput(string name, string value)
  {
    return $@"<input name=""{name}"" type=""hidden"" value=""{value}"" />{Environment.NewLine}";
  }
}