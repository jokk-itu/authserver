using Contracts.RegisterUser;
using Contracts.ResetPassword;
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
      [FromBody] PostRegisterUserRequest request)
  {
    var identityResult = await _manager.CreateAsync(new IdentityUser
    {
      UserName = request.Username,
      NormalizedUserName = request.Username.ToUpper(),
      Email = request.Email,
      NormalizedEmail = request.Email.ToUpper(),
      PhoneNumber = request.PhoneNumber
    }, request.Password);

    if (identityResult.Succeeded)
      return Ok();

    return BadRequest();
  }

  [HttpPost]
  [Route("reset/password")]
  public async Task<IActionResult> ResetPassword(
      [FromBody] PostResetPasswordRequest request)
  {
    var user = await _manager.FindByNameAsync(request.Username);
    var token = await _manager.GeneratePasswordResetTokenAsync(user);
    var result = await _manager.ResetPasswordAsync(user, token, request.Password);

    if (result.Succeeded)
      return Ok();

    return BadRequest();
  }
}