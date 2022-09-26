using System.Net;
using Contracts.AuthorizeCode;
using Microsoft.AspNetCore.Mvc;
using WebApp.Constants;
using Application;
using WebApp.Extensions;
using Microsoft.AspNetCore.Http.Extensions;
using Infrastructure.Requests.GetAuthorizationCode;
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
  public IActionResult Index()
  {
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
    CancellationToken cancellationToken = default)
  {
    var query = new GetAuthorizationCodeQuery
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
      State = state
    };
    var response = await _mediator.Send(query, cancellationToken: cancellationToken);
    return response.StatusCode switch
    {
      HttpStatusCode.Redirect when response.IsError() => 
        this.RedirectOAuthResult(redirectUri, state, response.ErrorCode!, response.ErrorDescription!),
      HttpStatusCode.BadRequest when response.IsError() =>
        this.BadOAuthResult(response.ErrorCode!, response.ErrorDescription!),
      HttpStatusCode.Redirect => Redirect($"{redirectUri}{GetCodeQuery(response)}"),
      _ => this.BadOAuthResult(ErrorCode.ServerError)
    };
  }

  private QueryString GetCodeQuery(GetAuthorizationCodeResponse response)
  {
    return new QueryBuilder
    {
      {ParameterNames.State, response.State},
      {ParameterNames.Code, response.Code}
    }.ToQueryString();
  }
}