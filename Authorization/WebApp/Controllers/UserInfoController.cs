using Domain.Constants;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using Domain;
using Infrastructure.Decoders.Abstractions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace WebApp.Controllers;

[ApiController]
[Route("connect/[controller]")]
public class UserInfoController : Controller
{
  private readonly ITokenDecoder _tokenDecoder;
  private readonly UserManager<User> _userManager;

  public UserInfoController(
    IMediator mediator,
    ITokenDecoder tokenDecoder,
    UserManager<User> userManager)
  {
    _tokenDecoder = tokenDecoder;
    _userManager = userManager;
  }

  [HttpGet]
  [Authorize]
  public async Task<IActionResult> GetAsync()
  {
    var accessToken = await HttpContext.GetTokenAsync(JwtBearerDefaults.AuthenticationScheme, TokenTypeConstants.AccessToken);
    if (string.IsNullOrWhiteSpace(accessToken))
      return Forbid(OpenIdConnectDefaults.AuthenticationScheme);

    var decodedAccessToken = _tokenDecoder.DecodeSignedToken(accessToken);
    if (decodedAccessToken is null)
      return Forbid(OpenIdConnectDefaults.AuthenticationScheme);

    var subjectIdentifier = decodedAccessToken.Claims.Single(x => x.Type == ClaimNameConstants.Sub);
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
