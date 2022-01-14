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
        [FromBody] TokenRequest request,
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

        if (!await _clientManager.IsValidGrantsAsync(clientId, new[] { request.GrantType }))
            return BadRequest("grant_type is not valid for client");

        if (!await _clientManager.IsValidRedirectUrisAsync(clientId, new[] { request.RedirectUri }))
            return BadRequest("redirect_uri is not valid for client");

        var codeFactory = new AuthorizationCodeTokenFactory(_configuration, _protector);
        string accessToken;
        string refreshToken;
        switch (request.GrantType)
        {
            case "refresh_token":
                if (string.IsNullOrEmpty(request.RefreshToken))
                    return BadRequest("refresh_token must not be null or empty");
                
                refreshToken = request.RefreshToken;
                var scope = await new RefreshTokenFactory(_configuration, _tokenValidationParameters)
                    .DecodeTokenAsync(refreshToken);
                var scopes = scope.Claims.Single(c => c.Type.Equals("scope")).Value.Split(' ');
                accessToken = await new AccessTokenFactory(_configuration, _tokenValidationParameters)
                    .GenerateTokenAsync(clientId, request.RedirectUri, scopes);
                break;
            case "authorization_code":
                if (string.IsNullOrEmpty(request.Code))
                    return BadRequest("code must not be null or empty");
                if (string.IsNullOrEmpty(request.CodeVerifier))
                    return BadRequest("code_verifier must not be null or empty");
                if (!await codeFactory.ValidateAsync(request.GrantType, request.Code, request.RedirectUri, clientId, request.CodeVerifier))
                    return BadRequest("authorization code is not valid");

                var decodedCode = await codeFactory.DecodeTokenAsync(request.Code);
                accessToken = await new AccessTokenFactory(_configuration, _tokenValidationParameters)
                    .GenerateTokenAsync(clientId, request.RedirectUri, decodedCode.Scopes);
                refreshToken = await new RefreshTokenFactory(_configuration, _tokenValidationParameters)
                    .GenerateTokenAsync(clientId, request.RedirectUri, decodedCode.Scopes);
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
        return Redirect(request.RedirectUri);
    }
}