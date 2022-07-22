using AuthorizationServer;
using Contracts.GetDiscovery;
using Contracts.GetJwksDocument;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using System.Text;

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
      AuthorizationEndpoint = $"{_identityConfiguration.ExternalIssuer}/connect/v1/authorize",
      TokenEndpoint = $"{_identityConfiguration.InternalIssuer}/connect/v1/token",
      JwksUri = $"{_identityConfiguration.InternalIssuer}/.well-known/jwks",
      Scopes = await _scopeManager.ReadScopesAsync()
    };
    return Ok(discoveryDocumentResponse);
  }

  [HttpGet]
  [Route("jwks")]
  public async Task<IActionResult> GetJwksDocumentAsync()
  {
    var jwk = await _jwkManager.GetJwkAsync();
    var publicKey = await _jwkManager.GetPublicKeyAsync(jwk);
    var modulus = Encoding.Default.GetString(publicKey.Modulus!);
    var exponent = Encoding.Default.GetString(publicKey.Exponent!);
    var jwksDocumentResponse = new GetJwksDocumentResponse
    {
      Keys = new JwkDto[]
      {
        new JwkDto
        {
          KeyId = jwk.KeyId,
          Modulus = Base64UrlEncoder.Encode(modulus),
          Exponent = Base64UrlEncoder.Encode(exponent)
        }
      }
    };
    return Ok(jwksDocumentResponse);
  }
}
