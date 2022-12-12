using System.Text;
using WebApp.Constants;

namespace WebApp;
public static class FormPostBuilder
{
  private const string _end = @"
</form>
</body>
</html>";

  public static string BuildAuthorizationCodeResponse(string redirectUri, string state, string code, string iss)
  {
    var formBuilder = new StringBuilder();
    formBuilder.Append(AddStart(redirectUri));
    formBuilder.Append(AddInput(ParameterNames.State, state));
    formBuilder.Append(AddInput(ParameterNames.Code, code));
    formBuilder.Append(AddInput(ParameterNames.Issuer, iss));
    formBuilder.Append(_end);
    return formBuilder.ToString();
  }

  public static string BuildErrorResponse(string redirectUri, string state, string? error, string? errorDescription, string iss)
  {
    var formBuilder = new StringBuilder();
    formBuilder.Append(AddStart(redirectUri));
    formBuilder.Append(AddInput(ParameterNames.State, state));
    if (!string.IsNullOrWhiteSpace(error))
    {
      formBuilder.Append(AddInput(ParameterNames.Error, error));
    }

    if (!string.IsNullOrWhiteSpace(errorDescription))
    {
      formBuilder.Append(AddInput(ParameterNames.ErrorDescription, errorDescription));
    }
    formBuilder.Append(AddInput(ParameterNames.Issuer, iss));
    formBuilder.Append(_end);
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

  private static string AddInput(string name, string value)
  {
    return $@"<input name=""{name}"" type=""hidden"" value=""{value}"" />{Environment.NewLine}";
  }
}