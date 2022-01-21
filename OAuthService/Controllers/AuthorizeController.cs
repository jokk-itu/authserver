using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using OAuthService.Constants;
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
        _protector = protectorProvider.CreateProtector(_configuration.AuthorizationCodeSecret);
    }

    [HttpPost]
    [Route("authorize")]
    public async Task<IActionResult> Authorize(
        AuthorizeRequest request)
    {
        if (!Uri.IsWellFormedUriString(request.RedirectUri, UriKind.Absolute))
            return BadRequest("redirect_uri is an invalid absolute uri");

        if (!await _clientManager.IsValidClientAsync(request.ClientId))
            return BadRequest("client_id does not exist");

        var scopes = request.Scope.Split(" ");
        if (!await _clientManager.IsValidScopesAsync(request.ClientId, scopes))
            return BadRequest("scopes are not valid for this client_id");

        if (!await _clientManager.IsValidRedirectUrisAsync(request.ClientId, new[] { request.RedirectUri }))
            return BadRequest("redirect_uri is not valid for this client_id");

        var responseTypes = request.ResponseType.Split(' ');
        if (!responseTypes.Any(rt => rt.Equals(ResponseType.CODE)))
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
            request.RedirectUri,
            scopes,
            request.ClientId,
            request.CodeChallenge,
            request.CodeChallengeMethod,
            user.Id);

        await _clientManager.SetTokenAsync(request.ClientId, _configuration.AuthorizationCodeSecret, code);
        return Redirect($"{request.RedirectUri}?code={code}&state={request.State}");
    }
}