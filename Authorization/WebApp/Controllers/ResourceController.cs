using Application;
using Infrastructure.Builders.Abstractions;
using Infrastructure.Requests.CreateResource;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.Constants;
using WebApp.Contracts;
using WebApp.Contracts.GetResourceInitialAccessToken;
using WebApp.Contracts.PostResource;
using WebApp.Controllers.Abstracts;

namespace WebApp.Controllers;

[Route("/connect/[controller]")]
public class ResourceController : OAuthControllerBase
{
  private readonly IMediator _mediator;
  private readonly ITokenBuilder _tokenBuilder;

  public ResourceController(IMediator mediator, ITokenBuilder tokenBuilder, IdentityConfiguration identityConfiguration) : base(identityConfiguration)
  {
    _mediator = mediator;
    _tokenBuilder = tokenBuilder;
  }

  [HttpGet]
  [Route("initial-token")]
  [AllowAnonymous]
  [ProducesResponseType(typeof(GetResourceInitialAccessToken), StatusCodes.Status200OK)]
  public IActionResult GeResourceInitialToken()
  {
    var token = _tokenBuilder.BuildResourceInitialAccessToken();
    return Ok(new GetResourceInitialAccessToken
    {
      AccessToken = token,
      ExpiresIn = 300
    });
  }

  [HttpPost]
  [Route("register")]
  [Authorize(Policy = AuthorizationConstants.ResourceRegistration, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
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
