using Application;
using Infrastructure.Builders.Abstractions;
using Infrastructure.Requests.CreateScope;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using WebApp.Contracts.PostScope;
using WebApp.Controllers.Abstracts;

namespace WebApp.Controllers;

[Route("connect/[controller]")]
public class ScopeController : OAuthControllerBase
{
  private readonly IMediator _mediator;

  public ScopeController(IMediator mediator, ITokenBuilder tokenBuilder, IdentityConfiguration identityConfiguration) : base(identityConfiguration)
  {
    _mediator = mediator;
  }

  [HttpPost]
  [Route("register")]
  [ProducesResponseType(typeof(PostScopeResponse), StatusCodes.Status201Created)]
  public async Task<IActionResult> Post([FromBody] PostScopeRequest request,
    CancellationToken cancellationToken = default)
  {
    var command = new CreateScopeCommand
    {
      ScopeName = request.ScopeName
    };
    var response = await _mediator.Send(command, cancellationToken: cancellationToken);

    if (response.IsError())
    {
      return BadOAuthResult(response.ErrorCode!, response.ErrorDescription!);
    }

    var uri = $"{Request.Scheme}://{Request.Host}/connect/scope/configuration";
    return Created(new Uri(uri), new PostScopeResponse
    {
      Id = response.Id,
      ScopeName = response.ScopeName,
      ScopeRegistrationAccessToken = response.ScopeRegistrationAccessToken
    });
  }
}