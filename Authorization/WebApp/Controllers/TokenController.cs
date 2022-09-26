﻿using Infrastructure;
using Infrastructure.Repositories;
using Contracts.PostToken;
using Microsoft.AspNetCore.Mvc;
using WebApp.Extensions;
using Domain.Constants;
using Domain;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System.Text.RegularExpressions;
using Application;
using WebApp.Contracts.PostToken;
using Infrastructure.Factories;
using Infrastructure.Builders.Abstractions;
using Infrastructure.Decoders.Abstractions;

namespace WebApp.Controllers;

[ApiController]
[Route("connect/v1/[controller]")]
public class TokenController : ControllerBase
{
  private readonly ClientManager _clientManager;
  private readonly CodeFactory _codeFactory;
  private readonly IdentityConfiguration _authenticationConfiguration;
  private readonly CodeManager _codeManager;
  private readonly ILogger<TokenController> _logger;
  private readonly ITokenBuilder _tokenBuilder;
  private readonly ITokenDecoder _tokenDecoder;

  public TokenController(
     ClientManager clientManager,
     ITokenBuilder tokenBuilder,
     ITokenDecoder tokenDecoder,
     CodeFactory codeFactory,
     IdentityConfiguration authenticationConfiguration,
     CodeManager codeManager,
     ILogger<TokenController> logger)
  {
    _clientManager = clientManager;
    _tokenBuilder = tokenBuilder;
    _tokenDecoder = tokenDecoder;
    _codeFactory = codeFactory;
    _authenticationConfiguration = authenticationConfiguration;
    _codeManager = codeManager;
    _logger = logger;
  }

  [HttpPost]
  [Consumes("application/x-www-form-urlencoded")]
  [ProducesResponseType(typeof(PostTokenResponse), StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  public async Task<IActionResult> PostAsync(
    [FromForm] PostTokenRequest request,
    CancellationToken cancellationToken = default)
  {
    var client = await _clientManager.ReadClientAsync(request.ClientId, cancellationToken: cancellationToken);
    if (client is null)
      return this.BadOAuthResult(ErrorCode.InvalidClient);

    if (!_clientManager.Login(request.ClientSecret, client))
        return this.BadOAuthResult(ErrorCode.InvalidClient);

    return request.GrantType switch
    {
      OpenIdConnectGrantTypes.AuthorizationCode => await PostAuthorizeAsync(request, client, cancellationToken: cancellationToken),
      OpenIdConnectGrantTypes.RefreshToken => await PostRefreshAsync(request, cancellationToken: cancellationToken),
      _ => this.BadOAuthResult(ErrorCode.UnsupportedGrantType)
    };
  }

  private async Task<IActionResult> PostRefreshAsync(
    PostTokenRequest request,
    CancellationToken cancellationToken = default)
  {
    if (string.IsNullOrWhiteSpace(request.RefreshToken))
      return this.BadOAuthResult(ErrorCode.InvalidRequest);

    var decodedRefreshToken = _tokenDecoder.DecodeToken(request.RefreshToken);
    if (decodedRefreshToken is null)
      return this.BadOAuthResult(ErrorCode.InvalidRequest);

    var scopes = decodedRefreshToken.Claims
      .Single(c => c.Type.Equals(ClaimNameConstants.Scope))
      .Value.Split(' ');

    var requestScopes = request.Scope?.Split(' ');

    if (requestScopes is not null
      && requestScopes.All(scope => scopes.Contains(scope)))
    {
      //Check every scope against the scope in refresh_token
      //If any new scope is not present in refresh_token
      //then revoke refresh_token and return BadRequest
    }

    var accessToken = await _tokenBuilder.BuildRefreshTokenAsync(request.ClientId, scopes, decodedRefreshToken.Subject, cancellationToken);

    return Ok(new PostTokenResponse
    {
      AccessToken = accessToken,
      RefreshToken = request.RefreshToken,
      ExpiresIn = _authenticationConfiguration.AccessTokenExpiration
    });
  }

  private async Task<IActionResult> PostAuthorizeAsync(
      PostTokenRequest request,
      Client client,
      CancellationToken cancellationToken = default)
  {
    if (string.IsNullOrWhiteSpace(request.RedirectUri))
      return this.BadOAuthResult(ErrorCode.InvalidRequest);

    if (string.IsNullOrWhiteSpace(request.Code))
      return this.BadOAuthResult(ErrorCode.InvalidRequest);

    if (string.IsNullOrWhiteSpace(request.CodeVerifier) || !Regex.IsMatch(request.CodeVerifier, "^[0-9a-zA-Z-_~.]{43,128}$"))
      return this.BadOAuthResult(ErrorCode.InvalidRequest);

    var decodedAuthorizationCode = _codeFactory.DecodeCode(request.Code);
    if (decodedAuthorizationCode is null)
      return this.BadOAuthResult(ErrorCode.InvalidRequest);

    var code = await _codeManager.ReadCodeAsync(request.Code, cancellationToken: cancellationToken);
    if (code is not null && code.IsRedeemed)
    {
      _logger.LogWarning("Code {@Code} replay detected", code);
      return this.BadOAuthResult(ErrorCode.AccessDenied);
    }

    /*if (!_clientManager.IsAuthorizedRedirectUris(client, new[] { request.RedirectUri }))
      return this.BadOAuthResult(ErrorCode.UnauthorizedClient);*/

    if (!await _codeFactory.ValidateAsync(request.Code, request.RedirectUri, request.ClientId, request.CodeVerifier))
      return this.BadOAuthResult(ErrorCode.InvalidRequest);

    var requestScopes = request.Scope?.Split(' ');
    if (requestScopes is not null
      && !requestScopes.All(scope => decodedAuthorizationCode.Scopes.Contains(scope)))
    {
      _logger.LogWarning("Scope {@Scope} deviates from code", requestScopes);
      return this.BadOAuthResult(ErrorCode.InvalidRequest);
    }

    var accessToken = await _tokenBuilder.BuildAccessTokenAsync(request.ClientId, decodedAuthorizationCode.Scopes, decodedAuthorizationCode.UserId, cancellationToken);
    var refreshToken = await _tokenBuilder.BuildRefreshTokenAsync(request.ClientId, decodedAuthorizationCode.Scopes, decodedAuthorizationCode.UserId, cancellationToken);
    var idToken = _tokenBuilder.BuildIdToken(request.ClientId, decodedAuthorizationCode.Scopes, decodedAuthorizationCode.Nonce, decodedAuthorizationCode.UserId);
    return Ok(new PostTokenResponse
    {
      AccessToken = accessToken,
      RefreshToken = refreshToken,
      IdToken = idToken,
      ExpiresIn = _authenticationConfiguration.AccessTokenExpiration
    });
  }
}