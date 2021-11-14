using System.Text;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
    [Route("login")]
    public async Task<IActionResult> Login()
    {
        var jokk = await _manager.Users.Where(u => u.UserName.Equals("jokk")).SingleAsync();
        //await _manager.AddPasswordAsync(jokk, "Kelsen_1");
        var token = await _manager.GenerateUserTokenAsync(jokk, "AccessTokenProvider", "access_token");
        await _manager.SetAuthenticationTokenAsync(jokk, "AuthService", "access_token", token);
        var got = await _manager.GetAuthenticationTokenAsync(jokk, "AuthService", "access_token");
        await _manager.RemoveAuthenticationTokenAsync(jokk, "AuthService", "access_token");
        
        await Task.Delay(500);
        return Ok();
    }

    [HttpPost]
    [Route("register")]
    public async Task<IActionResult> Register()
    {
        await Task.Delay(500);
        return Ok();
    }

    [HttpGet]
    [Route("logout")]
    public async Task<IActionResult> Logout()
    {
        await Task.Delay(500);
        return Ok();
    }
}