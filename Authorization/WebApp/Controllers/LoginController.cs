using System.Net;
using System.Security.Claims;
using Application;
using Domain.Constants;
using Infrastructure.Requests.Login;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
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
  [Consumes(MimeTypeConstants.FormUrlEncoded)]
  [ProducesResponseType(StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  public async Task<IActionResult> Post(
    PostLoginRequest request,
    [FromQuery(Name = ParameterNames.Prompt)] string prompt,
    CancellationToken cancellationToken = default)
  {
    var query = new LoginQuery
    {
      Username = request.Username,
      Password = request.Password
    };
    var loginResponse = await _mediator.Send(query, cancellationToken: cancellationToken);

    if (loginResponse.IsError())
    {
      return BadOAuthResult(loginResponse.ErrorCode, loginResponse.ErrorDescription);
    }

    var routeValues = HttpContext.Request.Query.ToRouteValueDictionary();
    var prompts = prompt.Split(' ');
    if (!prompts.Contains(PromptConstants.Consent))
    {
      return await GetAuthorizationCode(loginResponse.UserId, cancellationToken: cancellationToken);
    }

    var identity = new ClaimsIdentity(new[] { new Claim(ClaimNameConstants.Sub, loginResponse.UserId) },
      CookieAuthenticationDefaults.AuthenticationScheme);

    await HttpContext.SignInAsync(new ClaimsPrincipal(identity));
    return RedirectToAction(controllerName: "Consent", actionName: "Index", routeValues: routeValues);
  }

  private async Task<IActionResult> GetAuthorizationCode(string userId, CancellationToken cancellationToken = default)
  {
    var command = HttpContext.Request.Query.ToAuthorizationGrantCommand(userId);
    var authorizationGrantResponse = await _mediator.Send(command, cancellationToken: cancellationToken);
    return authorizationGrantResponse.StatusCode switch
    {
      HttpStatusCode.OK when authorizationGrantResponse.IsError() => 
        ErrorFormPostResult(command.RedirectUri, command.State, authorizationGrantResponse.ErrorCode, authorizationGrantResponse.ErrorDescription),
      HttpStatusCode.BadRequest when authorizationGrantResponse.IsError() =>
        BadOAuthResult(authorizationGrantResponse.ErrorCode, authorizationGrantResponse.ErrorDescription),
      HttpStatusCode.OK => AuthorizationCodeFormPostResult(command.RedirectUri, authorizationGrantResponse.State, authorizationGrantResponse.Code),
      _ => BadOAuthResult(ErrorCode.ServerError, "something went wrong")
    };
  }
}