using Infrastructure;
using Infrastructure.Repositories;
using Infrastructure.TokenFactories;
using Contracts.PostToken;
using Infrastructure.Factories.TokenFactories;
using Microsoft.AspNetCore.Mvc;
using WebApp.Extensions;
using Domain.Constants;
using WebApp.Constants;
using Domain;

namespace WebApp.Controllers;

[ApiController]
[Route("connect/v1/[controller]")]
public class TokenController : ControllerBase
{
  private readonly ClientManager _clientManager;
  private readonly AccessTokenFactory _accessTokenFactory;
  private readonly RefreshTokenFactory _refreshTokenFactory;
  private readonly IdTokenFactory _idTokenFactory;
  private readonly CodeFactory _codeFactory;
  private readonly IdentityConfiguration _authenticationConfiguration;
  private readonly CodeManager _codeManager;
  private readonly ILogger<TokenController> _logger;

  public TokenController(
     ClientManager clientManager,
     AccessTokenFactory accessTokenFactory,
     RefreshTokenFactory refreshTokenFactory,
     IdTokenFactory idTokenFactory,
     CodeFactory codeFactory,
     IdentityConfiguration authenticationConfiguration,
     CodeManager codeManager,
     ILogger<TokenController> logger)
  {
    _clientManager = clientManager;
    _accessTokenFactory = accessTokenFactory;
    _refreshTokenFactory = refreshTokenFactory;
    _idTokenFactory = idTokenFactory;
    _codeFactory = codeFactory;
    _authenticationConfiguration = authenticationConfiguration;
    _codeManager = codeManager;
    _logger = logger;
  }

  [HttpPost]
  [Consumes("application/x-www-form-urlencoded")]
  public async Task<IActionResult> PostAsync(
    PostTokenRequest request,
    CancellationToken cancellationToken = default)
  {
    var client = await _clientManager.ReadClientAsync(request.ClientId, cancellationToken: cancellationToken);
    if (client is null)
      return this.BadOAuthRequest(ErrorCode.InvalidClient);

    if (!_clientManager.Login(request.ClientSecret, client))
        return this.BadOAuthRequest(ErrorCode.InvalidClient);

    if (request.GrantType.Equals(GrantConstants.AuthorizationCode)) 
    {
      return await PostAuthorizeAsync(request, client, cancellationToken: cancellationToken);
    }
    else if(request.GrantType.Equals(GrantConstants.RefreshToken))
    {
      return await PostRefreshAsync(request, cancellationToken: cancellationToken);
    }

    return this.BadOAuthRequest(ErrorCode.UnsupportedGrantType);
  }

  public async Task<IActionResult> PostRefreshAsync(
    PostTokenRequest request,
    CancellationToken cancellationToken = default)
  {
    if (string.IsNullOrWhiteSpace(request.RefreshToken))
      return this.BadOAuthRequest(ErrorCode.InvalidRequest);

    var decodedRefreshToken = _refreshTokenFactory.DecodeToken(request.RefreshToken);
    var scopes = decodedRefreshToken.Claims
      .Single(c => c.Type.Equals(ClaimNameConstants.Scope))
      .Value.Split(' ');

    var requestScopes = request.Scope?.Split(' ');

    if (!string.IsNullOrWhiteSpace(request.Scope) && requestScopes is not null
      && !request.Scope.Split(' ').All(scope => scopes.Contains(scope)))
    {
      //Check every scope against the scope in refresh_token
      //If any new scope is not present in refresh_token
      //then revoke refresh_token and return BadRequest
    }

    var accessToken = await _accessTokenFactory.GenerateTokenAsync(request.ClientId, scopes, decodedRefreshToken.Subject);

    return Ok(new PostTokenResponse
    {
      AccessToken = accessToken,
      RefreshToken = request.RefreshToken,
      ExpiresIn = _authenticationConfiguration.AccessTokenExpiration
    });
  }

  public async Task<IActionResult> PostAuthorizeAsync(
      PostTokenRequest request,
      Client client,
      CancellationToken cancellationToken = default)
  {
    if (string.IsNullOrWhiteSpace(request.RedirectUri))
      return this.BadOAuthRequest(ErrorCode.InvalidRequest);

    if (string.IsNullOrWhiteSpace(request.Code))
      return this.BadOAuthRequest(ErrorCode.InvalidRequest);

    if (string.IsNullOrWhiteSpace(request.CodeVerifier))
      return this.BadOAuthRequest(ErrorCode.InvalidRequest);

    var code = await _codeManager.ReadCodeAsync(request.Code, cancellationToken: cancellationToken);
    var decodedAuthorizationCode = _codeFactory.DecodeCode(request.Code);
    if (code is not null && code.IsRedeemed)
    {
      _logger.LogWarning("Code {@Code} replay detected", decodedAuthorizationCode);
      return this.BadOAuthRequest(ErrorCode.AccessDenied);
    }

    if (!_clientManager.IsAuthorizedRedirectUris(client, new[] { request.RedirectUri }))
      return this.BadOAuthRequest(ErrorCode.UnauthorizedClient);

    if (!await _codeFactory.ValidateAsync(request.Code, request.RedirectUri, request.ClientId, request.CodeVerifier))
      return this.BadOAuthRequest(ErrorCode.InvalidRequest);

    if (!string.IsNullOrWhiteSpace(request.Scope))
    {
      //Check every scope against the scope in authorization_code
      //If any deviates then return BadRequest
    }

    var accessToken = await _accessTokenFactory.GenerateTokenAsync(request.ClientId, decodedAuthorizationCode.Scopes, decodedAuthorizationCode.UserId);
    var refreshToken = await _refreshTokenFactory.GenerateTokenAsync(request.ClientId, decodedAuthorizationCode.Scopes, decodedAuthorizationCode.UserId);
    var idToken = await _idTokenFactory.GenerateTokenAsync(request.ClientId, decodedAuthorizationCode.Scopes, decodedAuthorizationCode.Nonce, decodedAuthorizationCode.UserId);
    return Ok(new PostTokenResponse
    {
      AccessToken = accessToken,
      RefreshToken = refreshToken,
      IdToken = idToken,
      ExpiresIn = _authenticationConfiguration.AccessTokenExpiration
    });
  }
}