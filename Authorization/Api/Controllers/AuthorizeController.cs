using AuthorizationServer.Repositories;
using AuthorizationServer.TokenFactories;
using Contracts.AuthorizeCode;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text;

namespace AuthorizationServer.Controllers;

[ApiController]
[ApiVersion("1")]
[Route("connect/v{version:apiVersion}/[controller]")]
public class AuthorizeController : ControllerBase
{
  private readonly ClientManager _clientManager;
  private readonly AuthorizationCodeTokenFactory _authorizationCodeTokenFactory;
  private readonly SignInManager<IdentityUser> _signInManager;
  private readonly UserManager<IdentityUser> _userManager;

  public AuthorizeController(
      UserManager<IdentityUser> userManager,
      ClientManager clientManager,
      AuthorizationCodeTokenFactory authorizationCodeTokenFactory,
      SignInManager<IdentityUser> signInManager)
  {
    _clientManager = clientManager;
    _authorizationCodeTokenFactory = authorizationCodeTokenFactory;
    _signInManager = signInManager;
    _userManager = userManager;
  }

  [HttpPost]
  public async Task<IActionResult> PostAuthorizeAsync(
      PostAuthorizeCodeRequest request)
  {
    if (await _clientManager.IsValidClientAsync(request.ClientId) is null)
      return BadRequest("client_id does not exist");

    var scopes = request.Scope.Split(" ");
    if (!await _clientManager.IsValidScopesAsync(request.ClientId, scopes))
      return BadRequest("scopes are not valid for this client_id");

    if (!await _clientManager.IsValidRedirectUrisAsync(request.ClientId, new[] { request.RedirectUri }))
      return BadRequest("redirect_uri is not valid for this client_id");

    var signInResult = await _signInManager.PasswordSignInAsync(request.Username, request.Password, true, false);

    if(!signInResult.Succeeded) 
    {
      var errors = new StringBuilder();
      if (signInResult.IsLockedOut)
        errors.AppendLine("user is locked out");

      if (signInResult.IsNotAllowed)
        errors.AppendLine("User is not allowed to sign in");

      if (signInResult.RequiresTwoFactor)
        errors.AppendLine("User requires two factor authentication");

      return BadRequest(errors);
    }

    var user = await _userManager.FindByNameAsync(request.Username);
    var code = await _authorizationCodeTokenFactory.GenerateTokenAsync(
        request.RedirectUri,
        scopes,
        request.ClientId,
        request.CodeChallenge,
        request.CodeChallengeMethod,
        user.Id);

    var claims = new Claim[]
    {
      new (ClaimTypes.Name, user.UserName),
      new (ClaimTypes.Email, user.Email)
    };

    var claimsIdentity = new ClaimsIdentity(claims, OpenIdConnectDefaults.AuthenticationScheme);

    await HttpContext.SignInAsync(
        OpenIdConnectDefaults.AuthenticationScheme,
        new ClaimsPrincipal(claimsIdentity),
        new AuthenticationProperties());

    return Redirect($"{request.RedirectUri}?code={code}&state={request.State}");
  }
}