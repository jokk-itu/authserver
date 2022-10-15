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
      { ClaimNameConstants.Sub, user.Id }
    };

    if (scopes.Contains(ScopeConstants.Profile)) 
    {
      claims.Add(ClaimNameConstants.Name, $"{user.FirstName} {user.LastName}");

      claims.Add(ClaimNameConstants.FamilyName, user.LastName);

      claims.Add(ClaimNameConstants.GivenName, user.FirstName);

      claims.Add(ClaimNameConstants.Address, user.Address);

      var roles = await _userManager.GetRolesAsync(user);
      foreach (var role in roles)
      {
        claims.Add(ClaimNameConstants.Role, role);
      }

      claims.Add(ClaimNameConstants.Birthdate, user.Birthdate.ToString(CultureInfo.InvariantCulture));
      claims.Add(ClaimNameConstants.Locale, user.Locale);
    }

    if (scopes.Contains(ScopeConstants.Email)) 
      claims.Add(ClaimNameConstants.Email, user.Email);

    if (scopes.Contains(ScopeConstants.Phone))
      claims.Add(ClaimNameConstants.Phone, user.PhoneNumber);

    return Json(claims);
  }
}