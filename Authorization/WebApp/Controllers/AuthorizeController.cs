﻿using Microsoft.AspNetCore.Mvc;
using Application;
using Domain.Constants;
using Infrastructure.Requests.CreateAuthorizationGrant;
using Infrastructure.Requests.SilentLogin;
using MediatR;
using WebApp.Extensions;
using WebApp.Attributes;
using WebApp.Contracts;
using WebApp.Controllers.Abstracts;

namespace WebApp.Controllers;

[Route("connect/[controller]")]
public class AuthorizeController : OAuthControllerBase
{
  private readonly IMediator _mediator;

  public AuthorizeController(
    IdentityConfiguration identityConfiguration,
    IMediator mediator) : base(identityConfiguration)
  {
    _mediator = mediator;
  }

  [HttpGet]
  [SecurityHeader]
  public async Task<IActionResult> Get(AuthorizeRequest request, CancellationToken cancellationToken = default)
  {
    if (PromptConstants.Prompts.All(x => x != request.Prompt))
    {
      return BadOAuthResult(ErrorCode.InvalidRequest, "prompt is invalid");
    }

    var prompts = request.Prompt.Split(' ');
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
      return await GetSilentLogin(request, cancellationToken);
    }

    if (prompts.Contains(PromptConstants.Consent))
    {
      return RedirectToAction(controllerName: "Consent", actionName: "UpdateConsent", routeValues: routeValues);
    }

    return BadOAuthResult(ErrorCode.LoginRequired, "prompt must contain login");
  }

  private async Task<IActionResult> GetSilentLogin(AuthorizeRequest request, CancellationToken cancellationToken = default)
  {
    var query = new SilentLoginCommand
    {
      IdTokenHint = request.IdTokenHint,
      ClientId = request.ClientId,
      Nonce = request.Nonce,
      CodeChallenge = request.CodeChallenge,
      RedirectUri = request.RedirectUri,
      CodeChallengeMethod = request.CodeChallengeMethod,
      ResponseType = request.ResponseType,
      Scope = request.Scope,
      State = request.State
    };
    var response = await _mediator.Send(query, cancellationToken: cancellationToken);

    if (response.IsError())
    {
      return ErrorFormPostResult(request.RedirectUri, request.State, response.ErrorCode, response.ErrorDescription);
    }

    return AuthorizationCodeFormPostResult(request.RedirectUri, request.State, response.Code);
  }
}