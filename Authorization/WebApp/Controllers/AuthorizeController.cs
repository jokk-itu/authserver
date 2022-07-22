using AuthorizationServer.Repositories;
using AuthorizationServer.TokenFactories;
using Contracts.AuthorizeCode;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text;
using WebApp.Models;

namespace WebApp.Controllers;

[Route("connect/v1/[controller]")]
public class AuthorizeController : Controller
{
  private readonly UserManager<IdentityUser> _userManager;
  private readonly ClientManager _clientManager;
  private readonly AuthorizationCodeTokenFactory _authorizationCodeTokenFactory;

  public AuthorizeController(
    UserManager<IdentityUser> userManager,
    ClientManager clientManager,
    AuthorizationCodeTokenFactory authorizationCodeTokenFactory)
  {
    _userManager = userManager;
    _clientManager = clientManager;
    _authorizationCodeTokenFactory = authorizationCodeTokenFactory;
  }

  [HttpGet]
  public IActionResult Index(
    [FromQuery(Name = "response_type")] string responseType,
    [FromQuery(Name = "client_id")] string clientId,
    [FromQuery(Name = "redirect_uri")] string redirectUri,
    [FromQuery(Name = "scope")] string scope,
    [FromQuery(Name = "state")] string state,
    [FromQuery(Name = "code_challenge")] string codeChallenge,
    [FromQuery(Name = "code_challenge_method")] string codeChallengeMethod)
  {
    
    return View(new AuthorizeModel 
    {
      ResponseType = responseType,
      ClientId = clientId,
      RedirectUri = redirectUri,
      Scope = scope,
      State = state,
      CodeChallenge = codeChallenge,
      CodeChallengeMethod = codeChallengeMethod
    });
  }

  [ValidateAntiForgeryToken]
  [HttpPost]
  public async Task<IActionResult> PostAuthorizeAsync(
    [FromForm] PostAuthorizeCodeRequest request,
    [FromQuery(Name = "response_type")] string responseType,
    [FromQuery(Name = "client_id")] string clientId,
    [FromQuery(Name = "redirect_uri")] string redirectUri,
    [FromQuery(Name = "scope")] string scope,
    [FromQuery(Name = "state")] string state,
    [FromQuery(Name = "code_challenge")] string codeChallenge,
    [FromQuery(Name = "code_challenge_method")] string codeChallengeMethod)
  {
    if (await _clientManager.IsValidClientAsync(clientId) is null)
      return BadRequest("client_id does not exist");

    var scopes = scope.Split(" ");
    if (!await _clientManager.IsValidScopesAsync(clientId, scopes))
      return BadRequest("scopes are not valid for this client_id");

    if (!await _clientManager.IsValidRedirectUrisAsync(clientId, new[] { redirectUri }))
      return BadRequest("redirect_uri is not valid for this client_id");

    var user = await _userManager.FindByNameAsync(request.Username);
    var isPasswordValid = await _userManager.CheckPasswordAsync(user, request.Password);
    if (!isPasswordValid)
      return BadRequest("username or password is wrong");
    
    var code = await _authorizationCodeTokenFactory.GenerateTokenAsync(
        redirectUri,
        scopes,
        clientId,
        codeChallenge,
        codeChallengeMethod,
        user.Id);

    var claims = new Claim[]
    {
      new (ClaimTypes.GivenName, user.UserName),
      new (ClaimTypes.Name, user.UserName),
      new (ClaimTypes.Email, user.Email)
    };

    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

    await HttpContext.SignInAsync(
        CookieAuthenticationDefaults.AuthenticationScheme,
        new ClaimsPrincipal(claimsIdentity),
        new AuthenticationProperties
        {
          IsPersistent = true,
          IssuedUtc = DateTimeOffset.UtcNow
        });

    return Redirect($"{redirectUri}?code={code}&state={state}");
  }
}