using App.Models;
using App.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace App.Controllers;
public class HomeController : Controller
{
  private readonly WeatherService _weatherService;

  public HomeController(WeatherService weatherService)
  {
    _weatherService = weatherService;
  }

  public IActionResult Index()
  {
    return View();
  }

  [Authorize]
  public async Task<IActionResult> Weather()
  {
    var weatherDtos = await _weatherService.GetSecretAsync();
    return View(new WeatherModel { WeatherDtos = weatherDtos });
  }

  [Authorize]
  public IActionResult Account()
  {
    return View();
  }

  public IActionResult Login()
  {
    if (HttpContext.User.Identity?.IsAuthenticated ?? false)
    {
      return Redirect("/");
    }
    return Challenge(OpenIdConnectDefaults.AuthenticationScheme);
  }

  [Authorize]
  public IActionResult Logout()
  {
    return SignOut(
        CookieAuthenticationDefaults.AuthenticationScheme,
        OpenIdConnectDefaults.AuthenticationScheme);
  }
}