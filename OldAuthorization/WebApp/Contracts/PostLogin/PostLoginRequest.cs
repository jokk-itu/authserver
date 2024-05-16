using Microsoft.AspNetCore.Mvc;
using WebApp.Constants;

namespace WebApp.Contracts.PostLogin;
#nullable disable
public record PostLoginRequest
{
  [FromForm(Name = ParameterNames.Username)]
  public string Username { get; init; }

  [FromForm(Name = ParameterNames.Password)]
  public string Password { get; init; }
}