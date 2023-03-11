using Microsoft.AspNetCore.Mvc;
using Application;
using Domain.Constants;
using Infrastructure.Requests.SilentLogin;
using MediatR;
using WebApp.Extensions;
using WebApp.Attributes;
using WebApp.Controllers.Abstracts;
using WebApp.Context.Abstract;
using WebApp.Context.AuthorizeContext;

namespace WebApp.Controllers;

[Route("connect/[controller]")]
public class AuthorizeController : OAuthControllerBase
{
  private readonly IContextAccessor<AuthorizeContext> _contextAccessor;
  private readonly IMediator _mediator;

  public AuthorizeController(
    IContextAccessor<AuthorizeContext> contextAccessor,
    IdentityConfiguration identityConfiguration,
    IMediator mediator) : base(identityConfiguration)
  {
    _contextAccessor = contextAccessor;
    _mediator = mediator;
  }

  [HttpGet]
  [SecurityHeader]
  public async Task<IActionResult> Get(CancellationToken cancellationToken = default)
  {
    var context = await _contextAccessor.GetContext(HttpContext);
    if (PromptConstants.Prompts.All(x => x != context.Prompt))
    {
      return BadOAuthResult(ErrorCode.InvalidRequest, "prompt is invalid");
    }

    var prompts = context.Prompt.Split(' ');
    var routeValues = HttpContext.Request.Query.ToRouteValueDictionary();
    if (prompts.Contains(PromptConstants.Create))
    {
      return RedirectToAction(controllerName: "Register", actionName: "Index", routeValues: routeValues);
    }

    if (prompts.Contains(PromptConstants.Login))
    {
      return RedirectToAction(controllerName: "Login", actionName: "Index", routeValues: routeValues);
    }

    if (prompts.Contains(PromptConstants.None))
    {
      return await GetSilentLogin(context, cancellationToken);
    }

    if (prompts.Contains(PromptConstants.Consent))
    {
      return RedirectToAction(controllerName: "Consent", actionName: "UpdateConsent", routeValues: routeValues);
    }

    return BadOAuthResult(ErrorCode.LoginRequired, "prompt must contain login");
  }

  private async Task<IActionResult> GetSilentLogin(AuthorizeContext context, CancellationToken cancellationToken = default)
  {
    var query = new SilentLoginCommand
    {
      IdTokenHint = context.IdTokenHint,
      ClientId = context.ClientId,
      Nonce = context.Nonce,
      CodeChallenge = context.CodeChallenge,
      RedirectUri = context.RedirectUri,
      CodeChallengeMethod = context.CodeChallengeMethod,
      ResponseType = context.ResponseType,
      Scope = context.Scope,
      State = context.State
    };
    var response = await _mediator.Send(query, cancellationToken: cancellationToken);

    if (response.IsError())
    {
      return ErrorFormPostResult(context.RedirectUri, context.State, response.ErrorCode, response.ErrorDescription);
    }

    return AuthorizationCodeFormPostResult(context.RedirectUri, context.State, response.Code);
  }
}