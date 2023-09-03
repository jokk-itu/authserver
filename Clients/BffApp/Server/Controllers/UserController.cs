using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared;

namespace Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
  [Authorize]
  [HttpGet]
  public IActionResult Get()
  {
    var userDto = new UserDto
    {
      Claims = HttpContext.User.Claims.Select(x => new ClaimDto
      {
        Type = x.Type,
        Value = x.Value,
      })
    };

    return Ok(userDto);
  }

  [HttpGet("login")]
  public IActionResult Login()
  {
    if (!HttpContext.User.Identity?.IsAuthenticated ?? false)
    {
      return Challenge(OpenIdConnectDefaults.AuthenticationScheme);
    }

    return Redirect("/");
  }

  [HttpGet("logout")]
  public IActionResult Logout()
  {
    return SignOut(
      CookieAuthenticationDefaults.AuthenticationScheme,
      OpenIdConnectDefaults.AuthenticationScheme);
  }
}