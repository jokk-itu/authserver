using Contracts.RegisterUser;
using Contracts.ResetPassword;
using Domain;
using Infrastructure.Factories.TokenFactories;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;

namespace WebApp.Controllers;

[Route("connect/v1/[controller]")]
public class AccountController : Controller
{
  private readonly UserManager<User> _userManager;
  private readonly AccessTokenFactory _accessTokenFactory;

  public AccountController(UserManager<User> userManager, AccessTokenFactory accessTokenFactory)
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
    var identityResult = await _userManager.CreateAsync(new User
    {
      GivenName = request.GivenName,
      FamilyName = request.FamilyName,
      MiddleName = request.MiddleName,
      Name = $"{request.GivenName}{(string.IsNullOrWhiteSpace(request.MiddleName) ? string.Empty : request.MiddleName)}{request.FamilyName}",
      Address = request.Address,
      NickName = request.NickName,
      Locale = request.Locale,
      Gender = request.Gender,
      Birthdate = request.BirthDate,
      UserName = request.Username,
      Email = request.Email,
      PhoneNumber = request.PhoneNumber,
      NormalizedEmail = request.Email.ToUpper(),
      NormalizedUserName = request.Username.ToUpper()
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
    var scopes = decodedAccessToken.Claims
      .Single(x => x.Type.Equals("scope")).Value
      .Split(' ');

    var claims = new Dictionary<string, string>
    {
      { JwtRegisteredClaimNames.Sub, user.Id }
    };

    if (scopes.Contains("profile")) 
    {
      claims.Add(JwtRegisteredClaimNames.Name, user.Name);
      claims.Add(JwtRegisteredClaimNames.FamilyName, user.FamilyName);
      claims.Add(JwtRegisteredClaimNames.GivenName, user.GivenName);
      
      if(user.MiddleName is not null)
        claims.Add("middle_name", user.MiddleName);

      if (user.NickName is not null)
        claims.Add("nickname", user.NickName);

      if(user.Gender is not null)
        claims.Add(JwtRegisteredClaimNames.Gender, user.Gender);

      claims.Add(JwtRegisteredClaimNames.Birthdate, user.Birthdate.ToString());
      claims.Add("locale", user.Locale);
    }

    if (scopes.Contains("email")) 
      claims.Add(JwtRegisteredClaimNames.Email, user.Email);

    if (scopes.Contains("address"))
      claims.Add("address", user.Address);

    if (scopes.Contains("phone"))
      claims.Add("phone", user.PhoneNumber);

    return Ok(JsonSerializer.Serialize(claims));
  }
}