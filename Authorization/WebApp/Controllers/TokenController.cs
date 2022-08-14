using AuthorizationServer;
using AuthorizationServer.Repositories;
using AuthorizationServer.TokenFactories;
using Contracts.PostToken;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.Mvc;
using WebApp.Extensions;

namespace WebApp.Controllers;

[ApiController]
[Route("connect/v1/[controller]")]
public class TokenController : ControllerBase
{
  private readonly ClientManager _clientManager;
  private readonly AccessTokenFactory _accessTokenFactory;
  private readonly RefreshTokenFactory _refreshTokenFactory;
  private readonly IdTokenFactory _idTokenFactory;
  private readonly AuthorizationCodeTokenFactory _authorizationCodeTokenFactory;
  private readonly JwkManager _jwkManager;
  private readonly IdentityConfiguration _authenticationConfiguration;

  public TokenController(
     ClientManager clientManager,
     AccessTokenFactory accessTokenFactory,
     RefreshTokenFactory refreshTokenFactory,
     IdTokenFactory idTokenFactory,
     AuthorizationCodeTokenFactory authorizationCodeTokenFactory,
     JwkManager jwkManager,
     IdentityConfiguration authenticationConfiguration)
  {
    _clientManager = clientManager;
    _accessTokenFactory = accessTokenFactory;
    _refreshTokenFactory = refreshTokenFactory;
    _idTokenFactory = idTokenFactory;
    _authorizationCodeTokenFactory = authorizationCodeTokenFactory;
    _jwkManager = jwkManager;
    _authenticationConfiguration = authenticationConfiguration;
  }

  [HttpPost]
  [Consumes("application/x-www-form-urlencoded")]
  public async Task<IActionResult> PostAsync(
    [FromForm] IFormCollection formCollection,
    CancellationToken cancellationToken = default)
  {
    var request = new PostTokenRequest();

    if (formCollection.TryGetValue("grant_type", out var grantType))
      request.GrantType = grantType;

    if (formCollection.TryGetValue("client_id", out var clientId))
      request.ClientId = clientId.DecodeFromFormUrl();

    if (formCollection.TryGetValue("client_secret", out var clientSecret))
      request.ClientSecret = clientSecret.DecodeFromFormUrl();

    if (formCollection.TryGetValue("code", out var code))
      request.Code = code.DecodeFromFormUrl();

    if (formCollection.TryGetValue("redirect_uri", out var redirectUri))
      request.RedirectUri = redirectUri;

    if (formCollection.TryGetValue("scope", out var scope))
      request.Scope = scope;

    if (formCollection.TryGetValue("code_verifier", out var codeVerifier))
      request.CodeVerifier = codeVerifier.DecodeFromFormUrl();

    if (formCollection.TryGetValue("refresh_token", out var refreshToken))
      request.RefreshToken = refreshToken.DecodeFromFormUrl();

    if (!await _clientManager.IsValidClientAsync(request.ClientId!, request.ClientSecret!))
      return BadRequest("client_id or client_secret is not valid");

    if (!await _clientManager.IsValidGrantsAsync(request.ClientId!, new[] { request.GrantType }))
      return BadRequest("grant_type is not valid for client");

    if (request.GrantType.Equals("authorization_code"))
      return await PostAuthorizeAsync(request);
    else if (request.GrantType.Equals("refresh_token"))
      return await PostRefreshAsync(request);
    else
      throw new Exception();
  }

  public async Task<IActionResult> PostRefreshAsync(
    PostTokenRequest request)
  {
    var decodedRefreshToken = _refreshTokenFactory.DecodeToken(request.RefreshToken!);
    var scopes = decodedRefreshToken.Claims.Single(c => c.Type.Equals("scope")).Value.Split(' ');

    if (!string.IsNullOrWhiteSpace(request.Scope))
    {
      //Check every scope against the scope in refresh_token
      //If any new is not present in refresh_token then return BadRequest
    }

    var accessToken = await _accessTokenFactory.GenerateTokenAsync(request.ClientId!, scopes, decodedRefreshToken.Subject);

    return Ok(new PostTokenResponse
    {
      AccessToken = accessToken,
      RefreshToken = request.RefreshToken!,
      ExpiresIn = _authenticationConfiguration.AccessTokenExpiration
    });
  }

  public async Task<IActionResult> PostAuthorizeAsync(
      PostTokenRequest request)
  {
    if (!await _clientManager.IsValidRedirectUrisAsync(request.ClientId!, new[] { request.RedirectUri! }))
      return BadRequest("redirect_uri is not valid for client");

    if (!await _authorizationCodeTokenFactory.ValidateAsync(request.Code!, request.RedirectUri!, request.ClientId!, request.CodeVerifier!))
      return BadRequest("code is not valid");

    var decodedAuthorizationCode = _authorizationCodeTokenFactory.DecodeToken(request.Code!);

    if (!string.IsNullOrWhiteSpace(request.Scope))
    {
      //Check every scope against the scope in authorization_code
      //If any deviates then return BadRequest
    }

    var accessToken = await _accessTokenFactory.GenerateTokenAsync(request.ClientId!, decodedAuthorizationCode.Scopes, decodedAuthorizationCode.UserId);
    var refreshToken = await _refreshTokenFactory.GenerateTokenAsync(request.ClientId!, decodedAuthorizationCode.Scopes, decodedAuthorizationCode.UserId);
    var idToken = await _idTokenFactory.GenerateTokenAsync(request.ClientId!, decodedAuthorizationCode.Scopes, decodedAuthorizationCode.Nonce, decodedAuthorizationCode.UserId);
    return Ok(new PostTokenResponse
    {
      AccessToken = accessToken,
      RefreshToken = refreshToken,
      IdToken = idToken,
      ExpiresIn = _authenticationConfiguration.AccessTokenExpiration
    });
  }

  [HttpGet]
  [Route("{token}")]
  public async Task<IActionResult> VerifyAsync(string token)
  {
    await _jwkManager.VerifyAsync(token);
    return Ok();
  }
}
