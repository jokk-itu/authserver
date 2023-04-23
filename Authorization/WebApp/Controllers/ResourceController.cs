using Application;
using Infrastructure.Requests.CreateResource;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using WebApp.Contracts;
using WebApp.Contracts.PostResource;
using WebApp.Controllers.Abstracts;

namespace WebApp.Controllers;

[Route("/connect/[controller]")]
public class ResourceController : OAuthControllerBase
{
  private readonly IMediator _mediator;
  
  public ResourceController(IMediator mediator, IdentityConfiguration identityConfiguration) : base(identityConfiguration)
  {
    _mediator = mediator;
  }

  [HttpPost]
  [Route("register")]
  [ProducesResponseType(typeof(PostResourceRequest), StatusCodes.Status201Created)]
  [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
  public async Task<IActionResult> Post([FromBody] PostResourceRequest request, CancellationToken cancellationToken = default)
  {
    var response = await _mediator.Send(new CreateResourceCommand
    {
      ResourceName = request.ResourceName,
      Scopes = request.Scope.Split(' ')
    }, cancellationToken: cancellationToken);

    if (response.IsError())
    {
      return BadOAuthResult(response.ErrorCode!, response.ErrorDescription!);
    }

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
}
