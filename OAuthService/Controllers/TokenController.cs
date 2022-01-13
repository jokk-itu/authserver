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
        _clientManager = clientManager;
        _tokenValidationParameters = tokenValidationParameters;
        _protector = protectorProvider.CreateProtector("authorization_code");
    }

    [HttpPost]
    [Route("token")]
    public async Task<IActionResult> Token(
        [FromBody] AuthorizationCodeTokenRequest request,
        [FromHeader(Name = "Authorization")] string authorization)
    {
        var authorizationHeader = authorization.Split(' ');
        
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
        string accessToken;
        string refreshToken;
        switch (request.grant_type)
        {
            case "refresh_token":
                if (string.IsNullOrEmpty(request.refresh_token))
                    return BadRequest("refresh_token must not be null or empty");
                
                refreshToken = request.refresh_token;
                var scope = await new RefreshTokenFactory(_configuration, _tokenValidationParameters)
                    .DecodeTokenAsync(refreshToken);
                var scopes = scope.Claims.Single(c => c.Type.Equals("scope")).Value.Split(' ');
                accessToken = await new AccessTokenFactory(_configuration, _tokenValidationParameters)
                    .GenerateTokenAsync(clientId, request.redirect_uri, scopes);
                break;
            case "authorization_code":
                if (string.IsNullOrEmpty(request.code))
                    return BadRequest("code must not be null or empty");
                if (!await codeFactory.ValidateAsync(request.grant_type, request.code, request.redirect_uri, clientId))
                    return BadRequest("authorization code is not valid");

                var decodedCode = await codeFactory.DecodeTokenAsync(request.code);
                accessToken = await new AccessTokenFactory(_configuration, _tokenValidationParameters)
                    .GenerateTokenAsync(clientId, request.redirect_uri, decodedCode.Scopes);
                refreshToken = await new RefreshTokenFactory(_configuration, _tokenValidationParameters)
                    .GenerateTokenAsync(clientId, request.redirect_uri, decodedCode.Scopes);
                break;
            default:
                return BadRequest("grant_type is not recognized");
        }

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