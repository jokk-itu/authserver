using Infrastructure.Builders.Abstractions;
using Microsoft.AspNetCore.Mvc;
using WebApp.Contracts.GetDiscoveryDocument;
using Mapster;
using WebApp.Contracts.GetJwksDocument;

namespace WebApp.Controllers;

[ApiController]
[Route(".well-known")]
public class DiscoveryController : ControllerBase
{
  private readonly IDiscoveryBuilder _builder;

  public DiscoveryController(
    IDiscoveryBuilder builder)
  {
    _builder = builder;
  }

  [HttpGet]
  [Route("openid-configuration")]
  [ProducesResponseType(typeof(GetDiscoveryDocumentResponse), StatusCodes.Status200OK)]
  public async Task<IActionResult> GetDiscoveryDocument()
  {
    var document = await _builder.BuildDiscoveryDocument();
    return Ok(document.Adapt<GetDiscoveryDocumentResponse>());
  }

  [HttpGet]
  [Route("jwks")]
  [ProducesResponseType(typeof(GetJwksDocumentResponse), StatusCodes.Status200OK)]
  public async Task<IActionResult> GetJwksDocument()
  {
    var document = await _builder.BuildJwkDocument();
    return Ok(document.Adapt<GetJwksDocumentResponse>());
  }
}