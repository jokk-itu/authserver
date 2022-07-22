using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;
[Route("api/[controller]")]
[ApiController]
public class SecretController : ControllerBase
{
  public SecretController()
  {

  }

  [Authorize]
  [HttpGet]
  public async Task<IActionResult> GetAsync()
  {
    await Task.Delay(500);
    return Ok("This is secure data");
  }
}
