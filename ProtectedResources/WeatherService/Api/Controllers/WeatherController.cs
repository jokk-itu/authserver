using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;
[Route("api/[controller]")]
[ApiController]
public class WeatherController : ControllerBase
{
  private static readonly string[] _weatherTypes = { "Freezing", "Cold", "Mild", "Hot", "Scorching" };

  [Authorize(AuthenticationSchemes = OpenIdConnectDefaults.AuthenticationScheme, Policy = "Weather")]
  [HttpGet]
  public IActionResult Get()
  {
    return Ok(
      Enumerable
      .Range(0, Random.Shared.Next(7, 60))
      .Select(x => new 
      {
        Date = DateTime.Now.AddDays(x),
        Weather = _weatherTypes[Random.Shared.Next(0, _weatherTypes.Length-1)],
        Temperature = Random.Shared.Next(-50, 50)
      }));
  }
}