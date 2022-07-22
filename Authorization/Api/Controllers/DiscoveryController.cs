using AuthorizationServer;
using AuthorizationServer.Repositories;
using Contracts.GetDiscovery;
using Contracts.GetJwksDocument;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace Api.Controllers;

[ApiController]
[Route("/.well-known/")]
public class DiscoveryController : ControllerBase
{
  private readonly IdentityConfiguration _identityConfiguration;
  private readonly ScopeManager _scopeManager;

  public DiscoveryController(IdentityConfiguration identityConfiguration, ScopeManager scopeManager)
  {
    _identityConfiguration = identityConfiguration;
    _scopeManager = scopeManager;
  }

  [HttpGet]
  [Route("discoverydocument")]
  public async Task<IActionResult> GetDiscoveryDocumentAsync() 
  {
    var discoveryDocumentResponse = new GetDiscoveryDocumentResponse 
    {
      Issuer = _identityConfiguration.Issuer,
      AuthorizationEndpoint = $"{_identityConfiguration.Issuer}/authorize",
      TokenEndpoint = $"{_identityConfiguration.Issuer}/token",
      JwksUri = $"{_identityConfiguration.Issuer}/.well-known/jwksdocument",
      Scopes = await _scopeManager.ReadScopesAsync()
    };
    return Ok(discoveryDocumentResponse);
  }

  [HttpGet]
  [Route("jwksdocument")]
  public async Task<IActionResult> GetJwksDocumentAsync()
  {
    var modulus = Encoding.Default.GetString(_identityConfiguration.CryptoServicerProvider.ExportParameters(false).Modulus);
    var exponent = Encoding.Default.GetString(_identityConfiguration.CryptoServicerProvider.ExportParameters(false).Exponent);
    var jwksDocumentResponse = new GetJwksDocumentResponse 
    {
      Keys = new JwkDto[] 
      {
        new JwkDto 
        {
          Modulus = modulus,
          Exponent = exponent
        }
      }
    };
    return Ok(jwksDocumentResponse);
  }
}
