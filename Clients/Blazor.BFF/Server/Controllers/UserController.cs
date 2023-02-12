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
  [HttpGet]
  [AllowAnonymous]
  public IActionResult Get()
  {
    var isAuthenticated = HttpContext.User.Identity?.IsAuthenticated ?? false;
    if (!isAuthenticated)
    {
      return Ok();
    }

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
    return Challenge();
  }

  [HttpGet("logout")]
  public IActionResult Logout()
  {
    return SignOut(
      CookieAuthenticationDefaults.AuthenticationScheme,
      OpenIdConnectDefaults.AuthenticationScheme);
  }
}