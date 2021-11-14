using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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

    public TokenController(
        AuthenticationConfiguration configuration,
        UserManager<IdentityUser> userManager,
        ClientManager clientManager)
    {
        _configuration = configuration;
        _userManager = userManager;
        _clientManager = clientManager;
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

        if (!await _clientManager.IsValidClient(client_id))
            return BadRequest("client_id does not exist");

        var scopes = scope.Split(' ');
        if (!await _clientManager.IsValidScopes(client_id, scopes))
            return BadRequest("scopes are not valid for this client_id");

        if (!await _clientManager.IsValidRedirectUris(client_id, new[] { redirect_uri }))
            return BadRequest("redirect_uri is not valid for this client_id");

        var responseTypes = response_type.Split(' ');
        if (!responseTypes.Any(rt => rt.Equals("code")))
            return BadRequest("response_type must contain code");

        var user = await _userManager.FindByNameAsync(request.Username);
        if (user is null)
            return BadRequest("user information was wrong");
        var isCorrectPassword = _userManager.PasswordHasher
            .VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (isCorrectPassword == PasswordVerificationResult.Failed)
            return BadRequest("user information was wrong");
        if (isCorrectPassword == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.PasswordHash = _userManager.PasswordHasher.HashPassword(user, request.Password);
            await _userManager.UpdateAsync(user);
        }

        var codeFactory = new AuthorizationCodeTokenFactory(_configuration);
        var code = codeFactory.CreateToken(scopes, redirect_uri, client_id);
        return Redirect($"{redirect_uri}?code={code}&state={state}");
    }

    //Get access token from refresh token
    [HttpGet]
    [Route("token")]
    public async Task<IActionResult> Token(
        [FromQuery] string grant_type, //flow of access_token request
        [FromQuery] string code, //confirmation of authentication request
        [FromQuery] string redirect_uri,
        [FromQuery] string client_id,
        [FromQuery] string? refresh_token,
        [FromQuery] string? scope)
    {
        //Authenticate the client by the Authorization header (Basic)
        //Validate the code, since we issued it!!!
        //Validate the refresh token
        //Validate client_id
        //Validate that scope does not contain new scopes (if refreshing)
        //Validate the redirect_uri

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, "some_id"),
            new Claim(JwtRegisteredClaimNames.Aud, _configuration.Audience),
            new Claim(JwtRegisteredClaimNames.Iss, _configuration.Issuer)
        };

        //Access token
        var accessSecret = _configuration.TokenSecret;
        var accessKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(accessSecret));
        var accessSigningCredentials = new SigningCredentials(accessKey, SecurityAlgorithms.HmacSha256);
        var accessSecurityToken = new JwtSecurityToken(
            claims: claims,
            notBefore: DateTime.Now,
            expires: DateTime.Now.AddSeconds(_configuration.AccessTokenExpiration),
            signingCredentials: accessSigningCredentials);
        var accessToken = new JwtSecurityTokenHandler().WriteToken(accessSecurityToken);

        //Refresh token
        var refreshSecret = _configuration.TokenSecret;
        var refreshKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(refreshSecret));
        var refreshSigningCredentials = new SigningCredentials(refreshKey, SecurityAlgorithms.HmacSha256);
        var refreshSecurityToken = new JwtSecurityToken(
            claims: claims,
            notBefore: DateTime.Now,
            expires: DateTime.Now.AddDays(_configuration.RefreshTokenExpiration),
            signingCredentials: refreshSigningCredentials);
        var refreshToken = new JwtSecurityTokenHandler().WriteToken(refreshSecurityToken);

        await HttpContext.Response.WriteAsJsonAsync(new
        {
            access_token = accessToken,
            token_type = "Bearer",
            expires_in = _configuration.AccessTokenExpiration,
            refresh_token = refreshToken
        });
        return Redirect(redirect_uri);
    }
}