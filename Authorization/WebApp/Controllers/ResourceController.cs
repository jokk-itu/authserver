using Contracts;
using Infrastructure.Factories.TokenFactories;
using Infrastructure.Requests.CreateResource;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.Constants;
using WebApp.Contracts.GetClient;
using WebApp.Contracts.GetResourceInitialAccessToken;
using WebApp.Contracts.PostResource;
using WebApp.Contracts.PutResource;
using WebApp.Extensions;

namespace WebApp.Controllers;

[Route("/connect/[controller]")]
public class ResourceController : Controller
{
  private readonly IMediator _mediator;
  private readonly ResourceInitialAccessTokenFactory _resourceInitialAccessTokenFactory;

  public ResourceController(IMediator mediator, ResourceInitialAccessTokenFactory resourceInitialAccessTokenFactory)
  {
    _mediator = mediator;
    _resourceInitialAccessTokenFactory = resourceInitialAccessTokenFactory;
  }

  [HttpGet]
  [Route("initial-token")]
  [AllowAnonymous]
  [ProducesResponseType(typeof(GetResourceInitialAccessToken), StatusCodes.Status200OK)]
  public IActionResult GeResourceInitialToken()
  {
    var token = _resourceInitialAccessTokenFactory.GenerateToken();
    return Ok(new GetResourceInitialAccessToken
    {
      AccessToken = token,
      ExpiresIn = 300
    });
  }

  [HttpPost]
  [Route("register")]
  [Authorize(Policy = AuthorizationConstants.ResourceRegistration)]
  [ProducesResponseType(typeof(PostResourceRequest), StatusCodes.Status201Created)]
  [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
  public async Task<IActionResult> PostResourceAsync([FromBody] PostResourceRequest request, CancellationToken cancellationToken = default)
  {
    var response = await _mediator.Send(new CreateResourceCommand
    {
      ResourceName = request.ResourceName,
      Scopes = request.Scope.Split(' ')
    }, cancellationToken: cancellationToken);

    if (response.IsError())
      return this.BadOAuthResult(response.ErrorCode!, response.ErrorDescription!);

    var uri = $"{Request.Scheme}://{Request.Host}/connect/resource/configuration";
    return Created(new Uri(uri), new PostResourceResponse
    {
      ResourceId = response.ResourceId,
      Scope = response.Scope,
      ResourceName = response.ResourceName,
      ResourceRegistrationAccessToken = response.ResourceRegistrationAccessToken,
      ResourceSecret = response.ResourceSecret
    });
  }

  [HttpPut]
  [Authorize(Policy = AuthorizationConstants.ResourceConfiguration)]
  [Route("configuration")]
  [ProducesResponseType(typeof(PutResourceRequest), StatusCodes.Status200OK)]
  public async Task<IActionResult> PutResourceAsync([FromBody] PutResourceRequest request, CancellationToken cancellationToken = default)
  {
    return Ok();
  }

  [HttpDelete]
  [Authorize(Policy = AuthorizationConstants.ResourceConfiguration)]
  [Route("configuration")]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  public async Task<IActionResult> DeleteResourceAsync(CancellationToken cancellationToken = default)
  {
    return NoContent();
  }

  [HttpGet]
  [Authorize(Policy = AuthorizationConstants.ResourceConfiguration)]
  [Route("configuration")]
  [ProducesResponseType(typeof(GetClientResponse), StatusCodes.Status200OK)]
  public async Task<IActionResult> GetResourceAsync(CancellationToken cancellationToken = default)
  {
    return Ok();
  }
}
