﻿using Infrastructure;
using Contracts.GetJwksDocument;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using WebApp.Contracts.GetDiscoveryDocument;

namespace WebApp.Controllers;

[ApiController]
[Route("/.well-known")]
public class DiscoveryController : ControllerBase
{
  private readonly IdentityConfiguration _identityConfiguration;
  private readonly ScopeManager _scopeManager;
  private readonly JwkManager _jwkManager;

  public DiscoveryController(
    IdentityConfiguration identityConfiguration,
    ScopeManager scopeManager,
    JwkManager jwkManager)
  {
    _identityConfiguration = identityConfiguration;
    _scopeManager = scopeManager;
    _jwkManager = jwkManager;
  }

  [HttpGet]
  [Route("openid-configuration")]
  public async Task<IActionResult> GetDiscoveryDocumentAsync()
  {
    var discoveryDocumentResponse = new GetDiscoveryDocumentResponse
    {
      Issuer = _identityConfiguration.InternalIssuer,
      AuthorizationEndpoint = $"{_identityConfiguration.ExternalIssuer}/connect/authorize",
      TokenEndpoint = $"{_identityConfiguration.InternalIssuer}/connect/token",
      UserInfoEndpoint = $"{_identityConfiguration.InternalIssuer}/connect/userinfo",
      JwksUri = $"{_identityConfiguration.InternalIssuer}/.well-known/jwks",
      Scopes = (await _scopeManager.ReadScopesAsync()).Select(scope => scope.Name)
    };
    
    return Ok(discoveryDocumentResponse);
  }

  [HttpGet]
  [Route("jwks")]
  public IActionResult GetJwksDocument()
  {
    var response = new GetJwksDocumentResponse();
    foreach (var jwk in _jwkManager.Jwks)
    {
      response.Keys.Add(new JwkDto
      {
        KeyId = jwk.KeyId,
        Modulus = Base64UrlEncoder.Encode(jwk.Modulus),
        Exponent = Base64UrlEncoder.Encode(jwk.Exponent)
      });
    }
    return Ok(response);
  }
}