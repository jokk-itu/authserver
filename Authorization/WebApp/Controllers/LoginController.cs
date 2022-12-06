﻿using System.Net;
using Application;
using Domain.Constants;
using Infrastructure.Requests.CreateAuthorizationGrant;
using Infrastructure.Requests.GetLoginToken;
using MediatR;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using WebApp.Attributes;
using WebApp.Constants;
using WebApp.Contracts.PostLogin;
using WebApp.Extensions;

namespace WebApp.Controllers;

[Route("connect/[controller]")]
public class LoginController : OAuthControllerBase
{
  private readonly IMediator _mediator;

  public LoginController(IMediator mediator, IdentityConfiguration identityConfiguration) : base(identityConfiguration)
  {
    _mediator = mediator;
  }

  [HttpGet]
  [SecurityHeader]
  public IActionResult Index()
  {
    return View();
  }

  [HttpPost]
  [ValidateAntiForgeryToken]
  [SecurityHeader]
  [Consumes("application/x-www-form-urlencoded")]
  [ProducesResponseType(StatusCodes.Status302Found)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  public async Task<IActionResult> Post(
    PostLoginRequest request,
    [FromQuery(Name = ParameterNames.Prompt)] string prompt,
    CancellationToken cancellationToken = default)
  {
    var query = new GetLoginTokenQuery
    {
      Username = request.Username,
      Password = request.Password
    };
    var loginCodeResponse = await _mediator.Send(query, cancellationToken: cancellationToken);

    if (loginCodeResponse.IsError())
    {
      return BadOAuthResult(loginCodeResponse.ErrorCode, loginCodeResponse.ErrorDescription);
    }

    var routeValues = HttpContext.Request.Query.ToRouteValueDictionary();
    routeValues.Add(ParameterNames.LoginCode, loginCodeResponse.LoginCode);
    var prompts = prompt.Split(' ');
    if (prompts.Contains(PromptConstants.Consent))
    {
      return RedirectToAction(controllerName: "Consent", actionName: "Index", routeValues: routeValues);
    }

    return await GetAuthorizationCode(loginCodeResponse.LoginCode, cancellationToken: cancellationToken);
  }

  private async Task<IActionResult> GetAuthorizationCode(string loginCode, CancellationToken cancellationToken = default)
  {
    var command = HttpContext.Request.Query.ToAuthorizationGrantCommand(loginCode);
    var authorizationGrantResponse = await _mediator.Send(command, cancellationToken: cancellationToken);
    return authorizationGrantResponse.StatusCode switch
    {
      HttpStatusCode.Redirect when authorizationGrantResponse.IsError() => 
        RedirectOAuthResult(command.RedirectUri, command.State, authorizationGrantResponse.ErrorCode!, authorizationGrantResponse.ErrorDescription!),
      HttpStatusCode.BadRequest when authorizationGrantResponse.IsError() =>
        BadOAuthResult(authorizationGrantResponse.ErrorCode!, authorizationGrantResponse.ErrorDescription!),
      HttpStatusCode.OK => OkFormPostResult(command.RedirectUri, authorizationGrantResponse.State, authorizationGrantResponse.Code),
      _ => BadOAuthResult(ErrorCode.ServerError, "something went wrong")
    };
  }
}