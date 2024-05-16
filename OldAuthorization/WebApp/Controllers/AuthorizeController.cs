using System.Net;
using Microsoft.AspNetCore.Mvc;
using Application;
using Domain.Constants;
using Infrastructure.Requests.SilentCookieLogin;
using Infrastructure.Requests.SilentTokenLogin;
using Infrastructure.Services.Abstract;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using WebApp.Extensions;
using WebApp.Attributes;
using WebApp.Controllers.Abstracts;
using WebApp.Context.Abstract;
using WebApp.Context.AuthorizeContext;

namespace WebApp.Controllers;

[Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
[Route("connect/[controller]")]
public class AuthorizeController : OAuthControllerBase
{
  private readonly IContextAccessor<AuthorizeContext> _contextAccessor;
  private readonly IMediator _mediator;
  private readonly IClientService _clientService;

  public AuthorizeController(
    IContextAccessor<AuthorizeContext> contextAccessor,
    IdentityConfiguration identityConfiguration,
    IMediator mediator,
    IClientService clientService) : base(identityConfiguration)
  {
    _contextAccessor = contextAccessor;
    _mediator = mediator;
    _clientService = clientService;
  }

  [HttpGet]
  [AllowAnonymous]
  [SecurityHeader]
  public async Task<IActionResult> Get(CancellationToken cancellationToken)
  {
    var context = await _contextAccessor.GetContext(HttpContext);
    var clientValidation = await _clientService
      .ValidateRedirectAuthorization(context.ClientId, context.RedirectUri, context.State, cancellationToken);

    if (clientValidation.IsError())
    {
      return BadOAuthResult(clientValidation.ErrorCode, clientValidation.ErrorDescription);
    }

    if (string.IsNullOrWhiteSpace(context.Prompt))
    {
      return await CalculatePrompt(context, cancellationToken);
    }

    var routeValues = HttpContext.Request.Query.ToRouteValueDictionary();

    if (context.Prompt == PromptConstants.SelectAccount)
    {
      return RedirectToAction(controllerName: "SelectAccount", actionName: "Index", routeValues: routeValues);
    }

    if (context.Prompt.Split(' ').Contains(PromptConstants.Login))
    {
      return RedirectToAction(controllerName: "Login", actionName: "Index", routeValues: routeValues);
    }

    if (context.Prompt == PromptConstants.None && !string.IsNullOrWhiteSpace(context.IdTokenHint))
    {
      return await GetSilentTokenLogin(context, cancellationToken);
    }

    if (context.Prompt == PromptConstants.None && User.Identity is not null)
    {
      return await GetSilentCookieLogin(context, cancellationToken);
    }

    return ErrorFormPostResult(context.RedirectUri, context.State, ErrorCode.InvalidRequest, "prompt is invalid");
  }

  private async Task<IActionResult> GetSilentCookieLogin(AuthorizeContext context, CancellationToken cancellationToken)
  {
    var loggedInUsers = User.Identities.Count();
    if (loggedInUsers > 1)
    {
      return ErrorFormPostResult(context.RedirectUri, context.State,
        ErrorCode.AccountSelectionRequired, "more than one account has a session");
    }

    var userId = User.Claims.SingleOrDefault(x => x.Type == ClaimNameConstants.Sub)?.Value;
    var command = context.Adapt<SilentCookieLoginCommand>();
    command.UserId = userId;
    var response = await _mediator.Send(command, cancellationToken: cancellationToken);

    if (response.IsError())
    {
      if (response.StatusCode == HttpStatusCode.OK)
      {
        return ErrorFormPostResult(context.RedirectUri, context.State, response.ErrorCode,
          response.ErrorDescription);
      }

      return BadOAuthResult(response.ErrorCode, response.ErrorDescription);
    }

    return AuthorizationCodeFormPostResult(context.RedirectUri, context.State, response.Code);
  } 

  private async Task<IActionResult> GetSilentTokenLogin(AuthorizeContext context, CancellationToken cancellationToken)
  {
    var command = context.Adapt<SilentTokenLoginCommand>();
    var response = await _mediator.Send(command, cancellationToken: cancellationToken);

    if (response.IsError())
    {
      if (response.StatusCode == HttpStatusCode.OK)
      {
        return ErrorFormPostResult(context.RedirectUri, context.State, response.ErrorCode,
          response.ErrorDescription);
      }

      return BadOAuthResult(response.ErrorCode, response.ErrorDescription);
    }

    return AuthorizationCodeFormPostResult(context.RedirectUri, context.State, response.Code);
  }

  private async Task<IActionResult> CalculatePrompt(
    AuthorizeContext context,
    CancellationToken cancellationToken)
  {
    var routeValues = HttpContext.Request.Query.ToRouteValueDictionary();

    if (!string.IsNullOrWhiteSpace(context.IdTokenHint))
    {
      var command = context.Adapt<SilentTokenLoginCommand>();
      var response = await _mediator.Send(command, cancellationToken: cancellationToken);

      if (response.IsError())
      {
        return RedirectToAction(controllerName: "Login", actionName: "Index", routeValues: routeValues);
      }

      return AuthorizationCodeFormPostResult(context.RedirectUri, context.State, response.Code);
    }

    var amountOfSessions = User.Identities.Count();
    if (amountOfSessions > 1)
    {
      return RedirectToAction(controllerName: "SelectAccount", actionName: "Index", routeValues: routeValues);
    }

    if (amountOfSessions == 1)
    {
      var userId = User.Claims.SingleOrDefault(x => x.Type == ClaimNameConstants.Sub)?.Value;
      var command = context.Adapt<SilentCookieLoginCommand>();
      command.UserId = userId;
      var response = await _mediator.Send(command, cancellationToken: cancellationToken);

      if (response.IsError())
      {
        return RedirectToAction(controllerName: "Login", actionName: "Index", routeValues: routeValues);
      }

      return AuthorizationCodeFormPostResult(context.RedirectUri, context.State, response.Code);
    }

    return RedirectToAction(controllerName: "Login", actionName: "Index", routeValues: routeValues);
  }
}