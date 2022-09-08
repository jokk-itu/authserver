using Contracts.RegisterUser;
using Domain;
using Domain.Constants;
using Infrastructure.Factories.TokenFactories;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;

namespace WebApp.Controllers;

[Route("connect/v1/[controller]")]
public class AccountController : Controller
{
  private readonly UserManager<User> _userManager;
  private readonly AccessTokenFactory _accessTokenFactory;

  public AccountController(
    UserManager<User> userManager, 
    AccessTokenFactory accessTokenFactory)
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
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status200OK)]
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
  [Route("userinfo")]
  [Authorize]
  [ProducesResponseType(StatusCodes.Status200OK)]
  public async Task<IActionResult> UserInfo()
  {
    var accessToken = await HttpContext.GetTokenAsync(JwtBearerDefaults.AuthenticationScheme, TokenTypeConstants.AccessToken);
    if (string.IsNullOrWhiteSpace(accessToken))
      return Forbid(OpenIdConnectDefaults.AuthenticationScheme);

    var decodedAccessToken = await _accessTokenFactory.DecodeTokenAsync(accessToken);
    if (decodedAccessToken is null)
      return Forbid(OpenIdConnectDefaults.AuthenticationScheme);

    var subjectIdentifier = decodedAccessToken.Claims.Single(x => x.Type == JwtRegisteredClaimNames.Sub);
    var user = await _userManager.FindByIdAsync(subjectIdentifier.Value);
    var scopes = decodedAccessToken.Claims
      .Single(x => x.Type.Equals(ClaimNameConstants.Scope)).Value
      .Split(' ');

    var claims = new Dictionary<string, string>
    {
      { JwtRegisteredClaimNames.Sub, user.Id }
    };

    if (scopes.Contains(ScopeConstants.Profile)) 
    {
      claims.Add(ClaimTypes.Name, user.Name);
      claims.Add(JwtRegisteredClaimNames.Name, user.Name);

      claims.Add(ClaimTypes.Surname, user.FamilyName);
      claims.Add(JwtRegisteredClaimNames.FamilyName, user.FamilyName);

      claims.Add(ClaimTypes.GivenName, user.GivenName);
      claims.Add(JwtRegisteredClaimNames.GivenName, user.GivenName);

      claims.Add(ClaimTypes.StreetAddress, user.Address);

      var roles = await _userManager.GetRolesAsync(user);
      if (roles.Any())
        claims.Add(ClaimTypes.Role, JsonSerializer.Serialize(roles));

      if (user.MiddleName is not null)
        claims.Add(ClaimNameConstants.MiddleName, user.MiddleName);

      if (user.NickName is not null)
        claims.Add(ClaimNameConstants.Nickname, user.NickName);

      if(user.Gender is not null)
        claims.Add(ClaimTypes.Gender, user.Gender);

      claims.Add(JwtRegisteredClaimNames.Birthdate, user.Birthdate.ToString());
      claims.Add(ClaimTypes.Locality, user.Locale);
    }

    if (scopes.Contains(OpenIdConnectScope.Email)) 
      claims.Add(ClaimTypes.Email, user.Email);

    if (scopes.Contains(ScopeConstants.Phone))
      claims.Add(ClaimTypes.MobilePhone, user.PhoneNumber);

    return Json(claims);
  }
}