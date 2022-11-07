using System.Net;
using Contracts.AuthorizeCode;
using Microsoft.AspNetCore.Mvc;
using WebApp.Constants;
using Application;
using Domain.Constants;
using Infrastructure.Requests.CreateAuthorizationGrant;
using WebApp.Extensions;
using Microsoft.AspNetCore.Http.Extensions;
using MediatR;

namespace WebApp.Controllers;

[ApiController]
[Route("connect/[controller]")]
public class AuthorizeController : Controller
{
  private readonly IMediator _mediator;

  public AuthorizeController(IMediator mediator)
  {
    _mediator = mediator;
  }

  [HttpGet]
  public IActionResult Get(
    [FromQuery(Name = ParameterNames.Prompt)] string prompt)
  {
    if (PromptConstants.Prompts.All(x => x != prompt))
    {
      return this.BadOAuthResult(ErrorCode.InvalidRequest, "prompt is invalid");
    }

    var prompts = prompt.Split(' ');
    var routeValues = new RouteValueDictionary();
    foreach (var (key, value) in HttpContext.Request.Query)
    {
      routeValues.Add(key, value);
    }
    if (prompts.Contains(PromptConstants.Create))
    {
      return RedirectToAction(controllerName: "Register", actionName: "Index", routeValues: routeValues);
    }

    if (prompts.Contains(PromptConstants.Login))
    {
      return RedirectToAction(controllerName: "Login", actionName: "Index", routeValues: routeValues);
    }

    return this.BadOAuthResult(ErrorCode.InvalidRequest, "prompt is invalid");
  }

  [ValidateAntiForgeryToken]
  [HttpPost]
  [Consumes("application/x-www-form-urlencoded")]
  [ProducesResponseType(StatusCodes.Status302Found)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  public async Task<IActionResult> PostAsync(
    [FromForm] PostAuthorizeCodeRequest request,
    [FromQuery(Name = ParameterNames.ResponseType)] string responseType,
    [FromQuery(Name = ParameterNames.ClientId)] string clientId,
    [FromQuery(Name = ParameterNames.RedirectUri)] string redirectUri,
    [FromQuery(Name = ParameterNames.Scope)] string scope,
    [FromQuery(Name = ParameterNames.State)] string state,
    [FromQuery(Name = ParameterNames.CodeChallenge)] string codeChallenge,
    [FromQuery(Name = ParameterNames.CodeChallengeMethod)] string codeChallengeMethod,
    [FromQuery(Name = ParameterNames.Nonce)] string nonce,
    [FromQuery(Name = ParameterNames.MaxAge)] long maxAge,
    CancellationToken cancellationToken = default)
  {
    var query = new CreateAuthorizationGrantCommand
    {
      Username = request.Username,
      Password = request.Password,
      ResponseType = responseType,
      ClientId = clientId,
      CodeChallenge = codeChallenge,
      RedirectUri = redirectUri,
      CodeChallengeMethod = codeChallengeMethod,
      Nonce = nonce,
      Scopes = scope.Split(' '),
      State = state,
      MaxAge = maxAge
    };
    var response = await _mediator.Send(query, cancellationToken: cancellationToken);
    return response.StatusCode switch
    {
      HttpStatusCode.Redirect when response.IsError() => 
        this.RedirectOAuthResult(redirectUri, state, response.ErrorCode!, response.ErrorDescription!),
      HttpStatusCode.BadRequest when response.IsError() =>
        this.BadOAuthResult(response.ErrorCode!, response.ErrorDescription!),
      HttpStatusCode.Redirect => Redirect($"{redirectUri}{GetCodeQuery(response)}"),
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