using AuthorizationServer.Repositories;
using AuthorizationServer.TokenFactories;
using Contracts.PostToken;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace AuthorizationServer.Controllers;

[ApiController]
[ApiVersion("1")]
[Route("connect/v{version:apiVersion}/[controller]")]
public class TokenController : ControllerBase
{
  private readonly ClientManager _clientManager;
  private readonly UserManager<IdentityUser> _userManager;
  private readonly AccessTokenFactory _accessTokenFactory;
  private readonly RefreshTokenFactory _refreshTokenFactory;
  private readonly IdTokenFactory _idTokenFactory;
  private readonly AuthorizationCodeTokenFactory _authorizationCodeTokenFactory;
  private readonly IdentityConfiguration _authenticationConfiguration;

  public TokenController(
      ClientManager clientManager,
      UserManager<IdentityUser> userManager,
      AccessTokenFactory accessTokenFactory,
      RefreshTokenFactory refreshTokenFactory,
      IdTokenFactory idTokenFactory,
      AuthorizationCodeTokenFactory authorizationCodeTokenFactory,
      IdentityConfiguration authenticationConfiguration)
  {
    _clientManager = clientManager;
    _userManager = userManager;
    _accessTokenFactory = accessTokenFactory;
    _refreshTokenFactory = refreshTokenFactory;
    _idTokenFactory = idTokenFactory;
    _authorizationCodeTokenFactory = authorizationCodeTokenFactory;
    _authenticationConfiguration = authenticationConfiguration;
  }

  [HttpPost]
  public async Task<IActionResult> PostAsync(
    [FromBody] PostTokenRequest request, 
    [FromHeader(Name = "Authorization")] string authorization, 
    CancellationToken cancellationToken = default) 
  {
    var authorizationHeader = authorization.Split(' ');

    if (authorizationHeader.Length != 2 || !authorizationHeader[0].Equals("Basic"))
      return BadRequest("authorization header must be Basic and contain client_id");

    var basicAuthorization = Encoding.UTF8.GetString(Convert.FromBase64String(authorizationHeader[1])).Split(':');
    var clientId = basicAuthorization[0];
    var clientSecret = basicAuthorization[1];

    if (!await _clientManager.IsValidClientAsync(clientId, clientSecret))
      return BadRequest("clientId or clientSecret is not valid");

    if (!await _clientManager.IsValidGrantsAsync(clientId, new[] { request.GrantType }))
      return BadRequest("grant_type is not valid for client");

    if (request.GrantType.Equals("code"))
      return await PostAuthorizeAsync(request, clientId);
    else if (request.GrantType.Equals("refresh_token"))
      return await PostRefreshAsync(request, clientId);
    else
      throw new Exception();
  }

  public async Task<IActionResult> PostRefreshAsync(
    PostTokenRequest request,
    string clientId)
  {
    var decodedRefreshToken = await _refreshTokenFactory.DecodeTokenAsync(request.RefreshToken);
    var scopes = decodedRefreshToken.Claims.Single(c => c.Type.Equals("scope")).Value.Split(' ');

    var accessToken = await _accessTokenFactory.GenerateTokenAsync(clientId, scopes, decodedRefreshToken.Subject);
    await HttpContext.Response.WriteAsJsonAsync(new
    {
      access_token = accessToken,
      refresh_token = request.RefreshToken,
      token_type = "Bearer",
      expires_in = _authenticationConfiguration.AccessTokenExpiration
    });

    return Ok();
  }

  [HttpPost]
  [Route("authorize")]
  public async Task<IActionResult> PostAuthorizeAsync(
      [FromBody] PostTokenRequest request,
      string clientId)
  {
    if (!await _clientManager.IsValidRedirectUrisAsync(clientId, new[] { request.RedirectUri }))
      return BadRequest("redirect_uri is not valid for client");

    if (!await _authorizationCodeTokenFactory.ValidateAsync(request.GrantType, request.Code, request.RedirectUri, clientId, request.CodeVerifier))
      return BadRequest("authorization code is not valid");

    var decodedAuthorizationCode = await _authorizationCodeTokenFactory.DecodeTokenAsync(request.Code);
    var accessToken = await _accessTokenFactory.GenerateTokenAsync(clientId, decodedAuthorizationCode.Scopes, decodedAuthorizationCode.UserId);
    var refreshToken = await _refreshTokenFactory.GenerateTokenAsync(clientId, decodedAuthorizationCode.Scopes, decodedAuthorizationCode.UserId);
    var idToken = await _idTokenFactory.GenerateTokenAsync(clientId, decodedAuthorizationCode.Scopes, decodedAuthorizationCode.Nonce, decodedAuthorizationCode.UserId);
    await HttpContext.Response.WriteAsJsonAsync(new
    {
      access_token = accessToken,
      refresh_token = refreshToken,
      id_token = idToken,
      token_type = "Bearer",
      expires_in = _authenticationConfiguration.AccessTokenExpiration
    });

    return Redirect(request.RedirectUri);
  }
}