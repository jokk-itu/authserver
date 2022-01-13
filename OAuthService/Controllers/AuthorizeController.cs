using System.ComponentModel.DataAnnotations;
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
        [FromBody] AuthorizeRequest request,
        [Required(AllowEmptyStrings = false)] [FromQuery(Name = "response_type")] string responseType,
        [Required(AllowEmptyStrings = false)] [FromQuery(Name = "client_id")] string clientId,
        [Required(AllowEmptyStrings = false)] [FromQuery(Name = "redirect_uri")] string redirectUri,
        [Required(AllowEmptyStrings = false)] [FromQuery(Name = "scope")] string scope,
        [Required(AllowEmptyStrings = false)] [FromQuery(Name = "state")] string state)
    {
        if (!Uri.IsWellFormedUriString(redirectUri, UriKind.Absolute))
            return BadRequest("redirect_uri is an invalid absolute uri");

        if (!await _clientManager.IsValidClientAsync(clientId))
            return BadRequest("client_id does not exist");

        var scopes = scope.Split(' ');
        if (!await _clientManager.IsValidScopesAsync(clientId, scopes))
            return BadRequest("scopes are not valid for this client_id");

        if (!await _clientManager.IsValidRedirectUrisAsync(clientId, new[] { redirectUri }))
            return BadRequest("redirect_uri is not valid for this client_id");

        var responseTypes = responseType.Split(' ');
        if (!responseTypes.Any(rt => rt.Equals("code")))
            return BadRequest("response_type must contain code");

        var user = await _userManager.FindByNameAsync(request.Username);
        if (user?.PasswordHash is null)
            return BadRequest("user information was wrong");
        var isCorrectPassword = _userManager.PasswordHasher
            .VerifyHashedPassword(user, user.PasswordHash, request.Password);
        switch (isCorrectPassword)
        {
            case PasswordVerificationResult.Failed:
                return BadRequest("user information was wrong");
            case PasswordVerificationResult.SuccessRehashNeeded:
                user.PasswordHash = _userManager.PasswordHasher.HashPassword(user, request.Password);
                var result = await _userManager.UpdateAsync(user); //TODO VALIDATE RESULT
                break;
        }

        var codeFactory = new AuthorizationCodeTokenFactory(_configuration, _protector);
        var code = await codeFactory.GenerateTokenAsync(redirectUri, scopes, clientId);
        await _clientManager.SetTokenAsync(clientId, "authorization_code", code);
        return Redirect($"{redirectUri}?code={code}&state={state}");
    }
}