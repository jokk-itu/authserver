using AuthorizationServer.Requests;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AuthorizationServer.Controllers;

[ApiController]
[ApiVersion("1")]
[Route("connect/v{version:apiVersion}/[controller]")]
public class AccountController : ControllerBase
{
  private readonly UserManager<IdentityUser> _manager;

  public AccountController(UserManager<IdentityUser> manager)
  {
    _manager = manager;
  }

  [HttpPost]
  [Route("register")]
  public async Task<IActionResult> Register(
      [FromBody] RegisterUserRequest request)
  {
    var identityResult = await _manager.CreateAsync(new IdentityUser
    {
      UserName = request.Username,
      NormalizedUserName = request.Username.ToUpper(),
      Email = request.Email,
      NormalizedEmail = request.Email.ToUpper(),
      PhoneNumber = request.PhoneNumber
    }, request.Password);
    
    if(identityResult.Succeeded)
      return Ok();

    return BadRequest();
  }

  [HttpPost]
  [Route("reset/password")]
  public async Task<IActionResult> ResetPassword(
      [FromBody] ResetPasswordRequest request)
  {
    var user = await _manager.FindByNameAsync(request.Username);
    var token = await _manager.GeneratePasswordResetTokenAsync(user);
    await _manager.ResetPasswordAsync(user, token, request.Password);
    return Ok();
  }
}