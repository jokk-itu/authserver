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
    private readonly AuthenticationConfiguration _configuration;
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
        _configuration = configuration.Value;
        _userManager = userManager;
        _clientManager = clientManager;
        _tokenValidationParameters = tokenValidationParameters;
        _protector = protectorProvider.CreateProtector("authorization_code");
    }

    [HttpPost]
    [Route("authorize")]
    public async Task<IActionResult> Authorize(
        [FromBody] AuthorizeRequest request,
        [FromQuery(Name = "response_type")] string responseType,
        [FromQuery(Name = "client_id")] string clientId,
        [FromQuery(Name = "redirect_uri")] string redirectUri,
        [FromQuery(Name = "scope")] string scope,
        [FromQuery(Name = "state")] string state)
    {
        if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
            return BadRequest($"{nameof(request.Username)} or {nameof(request.Password)} is either null or empty");

        if (string.IsNullOrEmpty(redirectUri))
            return BadRequest("redirect_uri is null or empty");

        if (!Uri.IsWellFormedUriString(redirectUri, UriKind.Absolute))
            return BadRequest("redirect_uri is an invalid absolute uri");

        if (string.IsNullOrEmpty(state))
            return BadRequest("state must be set");

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

        var codeFactory = new AuthorizationCodeTokenFactory(_configuration, _protector);
        if (!await codeFactory.ValidateAsync(request.grant_type, request.code, request.redirect_uri, clientId))
            return BadRequest("authorization code is not valid");

        var decodedCode = await codeFactory.DecodeTokenAsync(request.code);
        var accessToken = await new AccessTokenFactory(_configuration, _tokenValidationParameters)
            .GenerateTokenAsync(clientId, request.redirect_uri, decodedCode.Scopes);
        var refreshToken = await new RefreshTokenFactory(_configuration, _tokenValidationParameters)
            .GenerateTokenAsync(clientId, request.redirect_uri, decodedCode.Scopes);

        await HttpContext.Response.WriteAsJsonAsync(new
        {
            access_token = accessToken,
            refresh_token = refreshToken,
            token_type = "Bearer",
            expires_in = _configuration.AccessTokenExpiration
        });
        return Redirect(request.redirect_uri);
    }
}