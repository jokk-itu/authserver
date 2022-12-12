using App.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.Services;

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
    var secret = await _weatherService.GetSecretAsync();
    return View(new WeatherModel { Secret = secret });
  }
}