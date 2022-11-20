﻿using System.Net;
using Application;
using Domain.Constants;
using Infrastructure.Requests.CreateAuthorizationGrant;
using Infrastructure.Requests.GetLoginToken;
using MediatR;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using WebApp.Constants;
using WebApp.Contracts.PostLogin;
using WebApp.Extensions;

namespace WebApp.Controllers;

[Route("connect/[controller]")]
public class LoginController : Controller
{
  private readonly IMediator _mediator;

  public LoginController(IMediator mediator)
  {
    _mediator = mediator;
  }

  [HttpGet]
  public IActionResult Index()
  {
    return View();
  }

  [HttpPost]
  [ValidateAntiForgeryToken]
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
    var loginTokenResponse = await _mediator.Send(query, cancellationToken: cancellationToken);

    if (loginTokenResponse.IsError())
      return this.BadOAuthResult(loginTokenResponse.ErrorCode, loginTokenResponse.ErrorDescription);

    var routeValues = HttpContext.Request.Query.ToRouteValueDictionary();
    routeValues.Add("login_token", loginTokenResponse.LoginToken);
    var prompts = prompt.Split(' ');
    if (prompts.Contains(PromptConstants.Consent))
      return RedirectToAction(controllerName: "Consent", actionName: "Index", routeValues: routeValues);

    return await GetAuthorizationCode(loginTokenResponse.LoginToken, cancellationToken: cancellationToken);
  }

  private async Task<IActionResult> GetAuthorizationCode(string loginToken, CancellationToken cancellationToken = default)
  {
    var command = HttpContext.Request.Query.ToAuthorizationGrantCommand(loginToken);
    var authorizationGrantResponse = await _mediator.Send(command, cancellationToken: cancellationToken);
    return authorizationGrantResponse.StatusCode switch
    {
      HttpStatusCode.Redirect when authorizationGrantResponse.IsError() => 
        this.RedirectOAuthResult(command.RedirectUri, command.State, authorizationGrantResponse.ErrorCode!, authorizationGrantResponse.ErrorDescription!),
      HttpStatusCode.BadRequest when authorizationGrantResponse.IsError() =>
        this.BadOAuthResult(authorizationGrantResponse.ErrorCode!, authorizationGrantResponse.ErrorDescription!),
      HttpStatusCode.Redirect => Redirect($"{command.RedirectUri}{GetCodeQuery(authorizationGrantResponse)}"),
      _ => this.BadOAuthResult(ErrorCode.ServerError, "something went wrong")
    };
  }

  private static QueryString GetCodeQuery(CreateAuthorizationGrantResponse response)
  {
    return new QueryBuilder
    {
      {ParameterNames.State, response.State},
      {ParameterNames.Code, response.Code}
    }.ToQueryString();
  }
}