using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;
[Route("api/[controller]")]
[ApiController]
public class AnonymousController : ControllerBase
{
  public AnonymousController()
  {

  }

  [AllowAnonymous]
  [HttpGet]
  public async Task<IActionResult> GetAsync()
  {
    await Task.Delay(500);
    return Ok("This is not secure data");
  }
}
