using System.Globalization;
using Contracts.RegisterUser;
using Domain;
using Domain.Constants;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Infrastructure.Decoders.Abstractions;

namespace WebApp.Controllers;

[Route("connect/v1/[controller]")]
public class AccountController : Controller
{
  private readonly UserManager<User> _userManager;
  private readonly ITokenDecoder _tokenDecoder;

  public AccountController(
    UserManager<User> userManager,
    ITokenDecoder tokenDecoder)
  {
    _userManager = userManager;
    _tokenDecoder = tokenDecoder;
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
      FirstName = request.GivenName,
      LastName = request.FamilyName,
      Address = request.Address,
      Locale = request.Locale,
      Birthdate = request.BirthDate,
      UserName = request.Username,
      Email = request.Email,
      PhoneNumber = request.PhoneNumber,
      NormalizedEmail = _userManager.NormalizeEmail(request.Email),
      NormalizedUserName = _userManager.NormalizeName(request.Username)
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

    var decodedAccessToken = _tokenDecoder.DecodeToken(accessToken);
    if (decodedAccessToken is null)
      return Forbid(OpenIdConnectDefaults.AuthenticationScheme);

    var subjectIdentifier = decodedAccessToken.Claims.Single(x => x.Type == JwtRegisteredClaimNames.Sub);
    var user = await _userManager.FindByIdAsync(subjectIdentifier.Value);
    var scopes = decodedAccessToken.Claims
      .Single(x => x.Type.Equals(ClaimNameConstants.Scope)).Value
      .Split(' ');

    var claims = new Dictionary<string, string>
    {
      { ClaimTypes.NameIdentifier, user.Id }
    };

    if (scopes.Contains(ScopeConstants.Profile)) 
    {
      claims.Add(ClaimTypes.Name, $"{user.FirstName} {user.LastName}");

      claims.Add(ClaimTypes.Surname, user.LastName);

      claims.Add(ClaimTypes.GivenName, user.FirstName);

      claims.Add(ClaimTypes.StreetAddress, user.Address);

      var roles = await _userManager.GetRolesAsync(user);
      foreach (var role in roles)
      {
        claims.Add(ClaimTypes.Role, role);
      }

      claims.Add(ClaimTypes.DateOfBirth, user.Birthdate.ToString(CultureInfo.InvariantCulture));
      claims.Add(ClaimTypes.Locality, user.Locale);
    }

    if (scopes.Contains(OpenIdConnectScope.Email)) 
      claims.Add(ClaimTypes.Email, user.Email);

    if (scopes.Contains(ScopeConstants.Phone))
      claims.Add(ClaimTypes.MobilePhone, user.PhoneNumber);

    return Json(claims);
  }
}