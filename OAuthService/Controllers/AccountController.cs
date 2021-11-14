using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OAuthService.Requests;

namespace OAuthService.Controllers;

[ApiController]
[ApiVersion("1")]
[ControllerName("account")]
[Route("oauth2/v{version:apiVersion}/[controller]")]
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
        await _manager.CreateAsync(new IdentityUser
        {
            UserName = request.Username,
            NormalizedUserName = request.Username.ToUpper(),
            Email = request.Email,
            NormalizedEmail = request.Email.ToUpper(),
            PhoneNumber = request.PhoneNumber
        }, request.Password);
        return Ok();
    }
}