using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using OAuthService.Repositories;
using OAuthService.Requests;
using OAuthService.TokenFactories;

namespace OAuthService.Controllers;

[ApiController]
[ApiVersion("1")]
[ControllerName("authorize")]
[Route("oauth2/{version:apiVersion}/[controller]")]
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
        _protector = protectorProvider.CreateProtector("authorization_code");
    }
    
    [HttpPost]
    [Route("authorize")]
    public async Task<IActionResult> Authorize(
        AuthorizeRequest request)
    {
        if (!Uri.IsWellFormedUriString(request.redirectUri, UriKind.Absolute))
            return BadRequest("redirect_uri is an invalid absolute uri");

        if (!await _clientManager.IsValidClientAsync(request.clientId))
            return BadRequest("client_id does not exist");

        var scopes = request.scope.Split(' ');
        if (!await _clientManager.IsValidScopesAsync(request.clientId, scopes))
            return BadRequest("scopes are not valid for this client_id");

        if (!await _clientManager.IsValidRedirectUrisAsync(request.clientId, new[] { request.redirectUri }))
            return BadRequest("redirect_uri is not valid for this client_id");

        var responseTypes = request.responseType.Split(' ');
        if (!responseTypes.Any(rt => rt.Equals("code")))
            return BadRequest("response_type must contain code");

        var user = await _userManager.FindByNameAsync(request.UserInformation.Username);
        if (user?.PasswordHash is null)
            return BadRequest("user information was wrong");
        var isCorrectPassword = _userManager.PasswordHasher
            .VerifyHashedPassword(user, user.PasswordHash, request.UserInformation.Password);
        switch (isCorrectPassword)
        {
            case PasswordVerificationResult.Failed:
                return BadRequest("user information was wrong");
            case PasswordVerificationResult.SuccessRehashNeeded:
                user.PasswordHash = _userManager.PasswordHasher.HashPassword(user, request.UserInformation.Password);
                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                    throw new Exception("Password cannot be re-hashed");
                break;
            case PasswordVerificationResult.Success:
                break;
            default:
                throw new
                    SwitchExpressionException($"{isCorrectPassword.GetType().Name} is not covered");
        }

        var codeFactory = new AuthorizationCodeTokenFactory(_configuration, _protector);
        var code = await codeFactory.GenerateTokenAsync(request.redirectUri, scopes, request.clientId, request.codeChallenge, request.codeChallengeMethod);
        await _clientManager.SetTokenAsync(request.clientId, "authorization_code", code);
        return Redirect($"{request.redirectUri}?code={code}&state={request.state}");
    }
}