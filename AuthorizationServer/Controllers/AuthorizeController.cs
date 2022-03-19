using System.Runtime.CompilerServices;
using AuthorizationServer.Exceptions;
using AuthorizationServer.Repositories;
using AuthorizationServer.Requests;
using AuthorizationServer.TokenFactories;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace AuthorizationServer.Controllers;

[ApiController]
[ApiVersion("1")]
[Route("connect/{version:apiVersion}/[controller]")]
public class AuthorizeController : ControllerBase
{
  private readonly ClientManager _clientManager;
  private readonly UserManager<IdentityUser> _userManager;
  private readonly IDataProtector _protector;
  private readonly AuthenticationConfiguration _configuration;

  public AuthorizeController(
      IOptions<AuthenticationConfiguration> configuration,
      UserManager<IdentityUser> userManager,
      ClientManager clientManager,
      IDataProtectionProvider protectorProvider)
  {
    _clientManager = clientManager;
    _userManager = userManager;
    _configuration = configuration.Value;
    _protector = protectorProvider.CreateProtector(_configuration.AuthorizationCodeSecret);
  }

  [HttpPost]
  [Route("authorize")]
  public async Task<IActionResult> Authorize(
      AuthorizeCodeRequest codeRequest)
  {
    if (!await _clientManager.IsValidClientAsync(codeRequest.ClientId))
      return BadRequest("client_id does not exist");

    var scopes = codeRequest.Scope.Split(" ");
    if (!await _clientManager.IsValidScopesAsync(codeRequest.ClientId, scopes))
      return BadRequest("scopes are not valid for this client_id");

    if (!await _clientManager.IsValidRedirectUrisAsync(codeRequest.ClientId, new[] { codeRequest.RedirectUri }))
      return BadRequest("redirect_uri is not valid for this client_id");

    var user = await _userManager.FindByNameAsync(codeRequest.UserInformation.Username);
    if (user?.PasswordHash is null)
      return BadRequest("user information was wrong");
    var isCorrectPassword = _userManager.PasswordHasher
        .VerifyHashedPassword(user, user.PasswordHash, codeRequest.UserInformation.Password);
    switch (isCorrectPassword)
    {
      case PasswordVerificationResult.Failed:
        return BadRequest("user information was wrong");
      case PasswordVerificationResult.SuccessRehashNeeded:
        user.PasswordHash = _userManager.PasswordHasher.HashPassword(user, codeRequest.UserInformation.Password);
        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
          throw new PasswordRehashFailedException("Password cannot be re-hashed");
        break;
      case PasswordVerificationResult.Success:
        break;
      default:
        throw new
            SwitchExpressionException($"{isCorrectPassword.GetType().Name} is not covered");
    }

    var codeFactory = new AuthorizationCodeTokenFactory(_configuration, _protector);
    var code = await codeFactory.GenerateTokenAsync(
        codeRequest.RedirectUri,
        scopes,
        codeRequest.ClientId,
        codeRequest.CodeChallenge,
        codeRequest.CodeChallengeMethod,
        user.Id);

    await _clientManager.SetTokenAsync(codeRequest.ClientId, _configuration.AuthorizationCodeSecret, code);
    return Redirect($"{codeRequest.RedirectUri}?code={code}&state={codeRequest.State}");
  }
}