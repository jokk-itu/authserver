using System.Text;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using OAuthService.Repositories;
using OAuthService.Requests;
using OAuthService.TokenFactories;

namespace OAuthService.Controllers;

[ApiController]
[ApiVersion("1")]
[ControllerName("token")]
[Route("oauth2/v{version:apiVersion}/[controller]")]
public class TokenController : ControllerBase
{
    private readonly IOptions<AuthenticationConfiguration> _configuration;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly ClientManager _clientManager;
    private readonly TokenValidationParameters _tokenValidationParameters;
    private readonly IDataProtector _protector;

    public TokenController(
        IOptions<AuthenticationConfiguration> configuration,
        UserManager<IdentityUser> userManager,
        ClientManager clientManager,
        IDataProtectionProvider protectorProvider,
        TokenValidationParameters tokenValidationParameters)
    {
        _configuration = configuration;
        _userManager = userManager;
        _clientManager = clientManager;
        _tokenValidationParameters = tokenValidationParameters;
        _protector = protectorProvider.CreateProtector("authorization_code");
    }

    [HttpPost]
    [Route("authorize")]
    public async Task<IActionResult> Authorize(
        [FromBody] AuthorizeRequest request,
        [FromQuery] string response_type,
        [FromQuery] string client_id,
        [FromQuery] string redirect_uri,
        [FromQuery] string scope,
        [FromQuery] string state)
    {
        if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
            return BadRequest($"{nameof(request.Username)} or {nameof(request.Password)} is either null or empty");

        if (string.IsNullOrEmpty(redirect_uri))
            return BadRequest("redirect_uri is null or empty");

        if (!Uri.IsWellFormedUriString(redirect_uri, UriKind.Absolute))
            return BadRequest("redirect_uri is an invalid absolute uri");

        if (string.IsNullOrEmpty(state))
            return BadRequest("state must be set");

        if (!await _clientManager.IsValidClientAsync(client_id))
            return BadRequest("client_id does not exist");

        var scopes = scope.Split(' ');
        if (!await _clientManager.IsValidScopesAsync(client_id, scopes))
            return BadRequest("scopes are not valid for this client_id");

        if (!await _clientManager.IsValidRedirectUrisAsync(client_id, new[] { redirect_uri }))
            return BadRequest("redirect_uri is not valid for this client_id");

        var responseTypes = response_type.Split(' ');
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

        var codeFactory = new AuthorizationCodeTokenFactory(_configuration.Value, _protector);
        var code = await codeFactory.GenerateTokenAsync(redirect_uri, scopes, client_id);
        await _clientManager.SetTokenAsync(client_id, "authorization_code", code);
        return Redirect($"{redirect_uri}?code={code}&state={state}");
    }
    
    [HttpPost]
    [Route("token")]
    public async Task<IActionResult> Token(
        [FromBody] AuthorizationCodeTokenRequest request,
        [FromHeader(Name = "Authorization")] string authorization)
    {
        var authorizationHeader = HttpContext.Request.Headers.Authorization
            .ToString()
            .Split(' ');
        
        if (authorizationHeader.Length != 2 || !authorizationHeader[0].Equals("Basic"))
            return BadRequest("authorization header must be Basic and contain client_id");
        
        var basicAuthorization = Encoding.UTF8.GetString(Convert.FromBase64String(authorizationHeader[1])).Split(':');
        var clientId = basicAuthorization[0];
        var clientSecret = basicAuthorization[1];
        var client = await _clientManager.FindClientByIdAsync(clientId);
        if (client is null)
            return BadRequest("client_id does not exist");

        if (!await _clientManager.IsValidGrantsAsync(clientId, new[] { request.grant_type }))
            return BadRequest("grant_type is not valid for client");

        if (!await _clientManager.IsValidRedirectUrisAsync(clientId, new[] { request.redirect_uri }))
            return BadRequest("redirect_uri is not valid for client");

        var codeFactory = new AuthorizationCodeTokenFactory(_configuration.Value, _protector);
        if (!await codeFactory.ValidateAsync(request.grant_type, request.code, request.redirect_uri, clientId))
            return BadRequest("authorization code is not valid");

        var decodedCode = await codeFactory.DecodeTokenAsync(request.code);
        var accessToken = await new AccessTokenFactory(_configuration.Value, _tokenValidationParameters)
            .GenerateTokenAsync(clientId, request.redirect_uri, decodedCode.Scopes);
        var refreshToken = await new RefreshTokenFactory(_configuration.Value, _tokenValidationParameters)
            .GenerateTokenAsync(clientId, request.redirect_uri, decodedCode.Scopes);

        await HttpContext.Response.WriteAsJsonAsync(new
        {
            access_token = accessToken,
            refresh_token = refreshToken,
            token_type = "Bearer",
            expires_in = _configuration.Value.AccessTokenExpiration
        });
        return Redirect(request.redirect_uri);
    }
}