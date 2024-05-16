using System.Net;
using Application;
using Infrastructure.Requests.EndSession;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using WebApp.Context.Abstract;
using WebApp.Context.EndSessionContext;
using WebApp.Controllers.Abstracts;

namespace WebApp.Controllers;

[Route("connect/end-session")]
public class EndSessionController : OAuthControllerBase
{
  private readonly IMediator _mediator;
  private readonly IContextAccessor<EndSessionContext> _contextAccessor;

  public EndSessionController(
    IMediator mediator,
    IContextAccessor<EndSessionContext> contextAccessor,
    IdentityConfiguration identityConfiguration) : base(identityConfiguration)
  {
    _mediator = mediator;
    _contextAccessor = contextAccessor;
  }

  [HttpGet]
  [HttpPost]
  public async Task<IActionResult> Logout(CancellationToken cancellationToken = default)
  {
    var context = await _contextAccessor.GetContext(HttpContext);
    var command = new EndSessionCommand
    {
      ClientId = context.ClientId,
      State = context.State,
      PostLogoutRedirectUri = context.PostLogoutRedirectUri,
      IdTokenHint = context.IdTokenHint
    };
    var response = await _mediator.Send(command, cancellationToken: cancellationToken);
    if (response.IsError())
    {
      return BadOAuthResult(response.ErrorCode, response.ErrorDescription);
    }

    return response.StatusCode switch
    {
      HttpStatusCode.OK => Ok(),
      HttpStatusCode.Redirect => LogoutRedirectResult(context.PostLogoutRedirectUri, context.State),
      _ => BadOAuthResult(ErrorCode.ServerError, "something went wrong"),
    };
  }
}