using Application;
using Infrastructure.Requests.TokenRevocation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using WebApp.Context.Abstract;
using WebApp.Context.RevocationContext;
using WebApp.Controllers.Abstracts;

namespace WebApp.Controllers;

[Route("connect/[controller]")]
public class RevokeController : OAuthControllerBase
{
  private readonly IMediator _mediator;
  private readonly IContextAccessor<RevocationContext> _contextAccessor;

  public RevokeController(
    IdentityConfiguration identityConfiguration,
    IMediator mediator,
    IContextAccessor<RevocationContext> contextAccessor)
    : base(identityConfiguration)
  {
    _mediator = mediator;
    _contextAccessor = contextAccessor;
  }

  [HttpPost]
  [ProducesResponseType(StatusCodes.Status200OK)]
  public async Task<IActionResult> Post(CancellationToken cancellationToken = default)
  {
    var context = await _contextAccessor.GetContext(HttpContext);
    var command = new TokenRevocationCommand
    {
      TokenTypeHint = context.TokenTypeHint,
      Token = context.Token,
      ClientAuthentications = context.ClientAuthentications
    };
    var response = await _mediator.Send(command, cancellationToken: cancellationToken);
    if (response.IsError())
    {
      return BadOAuthResult(response.ErrorCode, response.ErrorDescription);
    }
    return Ok();
  }
}