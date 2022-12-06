using System.Text;
using Infrastructure.Builders.Abstractions;

namespace Infrastructure.Builders;
public class FormPostBuilder : IFormPostBuilder
{
  public string BuildAuthorizationCodeResponse(string redirectUri, string state, string code, string iss)
  {
    var formBuilder = new StringBuilder();
    formBuilder.Append(AddStart(redirectUri));
    formBuilder.Append(AddInput("state", state));
    formBuilder.Append(AddInput("code", code));
    formBuilder.Append(AddInput("iss", iss));
    formBuilder.Append(AdddEnd());
    return formBuilder.ToString();
  }

  private string AddStart(string redirectUri)
  {
    return $@"
<!DOCTYPE html>
<html>
<head>
<title>Submit</title>
</head>
<body onload=""javascript:document.forms[0].submit()"">
<form method=""post"" action=""{redirectUri}"">
";
  }

  private string AdddEnd()
  {
    return @"
</form>
</body>
</html>";
  }

  private string AddInput(string name, string value)
  {
    return $@"<input type=""hidden"" name=""{name}"" value=""{value}""/>";
  }
}