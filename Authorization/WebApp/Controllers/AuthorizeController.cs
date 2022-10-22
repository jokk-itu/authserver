using System.Net;
using Contracts.AuthorizeCode;
using Microsoft.AspNetCore.Mvc;
using WebApp.Constants;
using Application;
using Infrastructure.Requests.CreateAuthorizationGrant;
using WebApp.Extensions;
using Microsoft.AspNetCore.Http.Extensions;
using MediatR;

namespace WebApp.Controllers;

[Route("connect/v1/[controller]")]
public class AuthorizeController : Controller
{
  private readonly IMediator _mediator;

  public AuthorizeController(IMediator mediator)
  {
    _mediator = mediator;
  }

  [HttpGet]
  public IActionResult Index(
    [FromQuery(Name = ParameterNames.Prompt)] string prompt)
  {
    //var prompts = prompt.Split(' ');
    // TODO DROP None support
    // TODO DROP select_account support
    // TODO Enable consent and login support
    return View();
  }

  [ValidateAntiForgeryToken]
  [HttpPost]
  [Consumes("application/x-www-form-urlencoded")]
  [ProducesResponseType(StatusCodes.Status302Found)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  public async Task<IActionResult> PostAuthorizeAsync(
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