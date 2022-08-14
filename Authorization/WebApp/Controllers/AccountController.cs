using AuthorizationServer.TokenFactories;
using Contracts.RegisterUser;
using Contracts.ResetPassword;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;

namespace WebApp.Controllers;

[Route("connect/v1/[controller]")]
public class AccountController : Controller
{
  private readonly UserManager<IdentityUser> _userManager;
  private readonly AccessTokenFactory _accessTokenFactory;

  public AccountController(UserManager<IdentityUser> userManager, AccessTokenFactory accessTokenFactory)
  {
    _userManager = userManager;
    _accessTokenFactory = accessTokenFactory;
  }

  [HttpGet]
  [Route("register")]
  public IActionResult Register()
  {
    return View();
  }

  [ValidateAntiForgeryToken]
  [HttpPost]
  [Route("register")]
  public async Task<IActionResult> Register(
    PostRegisterUserRequest request)
  {
    var identityResult = await _userManager.CreateAsync(new IdentityUser
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

  [HttpGet]
  [Route("reset/password")]
  public IActionResult ResetPassword()
  {
    return View();
  }

  [ValidateAntiForgeryToken]
  [HttpPost]
  [Route("reset/password")]
  public async Task<IActionResult> ResetPassword(
    PostResetPasswordRequest request)
  {
    var user = await _userManager.FindByNameAsync(request.Username);
    var token = await _userManager.GeneratePasswordResetTokenAsync(user);
    var result = await _userManager.ResetPasswordAsync(user, token, request.Password);

    if (result.Succeeded)
      return Ok();

    return BadRequest();
  }

  [HttpGet]
  [Route("userinfo")]
  [Authorize]
  public async Task<IActionResult> UserInfo() 
  {
    var accessToken = await HttpContext.GetTokenAsync(OpenIdConnectDefaults.AuthenticationScheme, "access_token");
    if (string.IsNullOrWhiteSpace(accessToken))
      return Forbid();

    var decodedAccessToken = _accessTokenFactory.DecodeToken(accessToken);
    var subjectIdentifier = decodedAccessToken.Claims.Single(x => x.Type == JwtRegisteredClaimNames.Sub);
    var user = await _userManager.FindByIdAsync(subjectIdentifier.Value);
    
    return Ok();
  }
}
