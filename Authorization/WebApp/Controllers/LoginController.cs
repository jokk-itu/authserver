using System.Net;
using System.Security.Claims;
using Application;
using Domain.Constants;
using Infrastructure.Requests.CreateAuthorizationGrant;
using Infrastructure.Requests.Login;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using WebApp.Attributes;
using WebApp.Constants;
using WebApp.Context.Abstract;
using WebApp.Context.AuthorizeContext;
using WebApp.Contracts.PostLogin;
using WebApp.Controllers.Abstracts;
using WebApp.Extensions;

namespace WebApp.Controllers;

[Route("connect/[controller]")]
public class LoginController : OAuthControllerBase
{
  private readonly IMediator _mediator;
  private readonly IContextAccessor<AuthorizeContext> _contextAccessor;

  public LoginController(
    IMediator mediator,
    IdentityConfiguration identityConfiguration,
    IContextAccessor<AuthorizeContext> contextAccessor) : base(identityConfiguration)
  {
    _mediator = mediator;
    _contextAccessor = contextAccessor;
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
    CancellationToken cancellationToken = default)
  {
    var context = await _contextAccessor.GetContext(HttpContext);
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

    var prompts = context.Prompt.Split(' ');
    if (!prompts.Contains(PromptConstants.Consent))
    {
      return await GetAuthorizationCode(context, loginResponse.UserId, cancellationToken: cancellationToken);
    }

    var identity = new ClaimsIdentity(new[] { new Claim(ClaimNameConstants.Sub, loginResponse.UserId) },
      CookieAuthenticationDefaults.AuthenticationScheme);

    await HttpContext.SignInAsync(new ClaimsPrincipal(identity));
    var routeValues = HttpContext.Request.Query.ToRouteValueDictionary();
    return RedirectToAction(controllerName: "Consent", actionName: "CreateConsent", routeValues: routeValues);
  }

  private async Task<IActionResult> GetAuthorizationCode(AuthorizeContext context, string userId, CancellationToken cancellationToken = default)
  {
    var command = new CreateAuthorizationGrantCommand
    {
      ClientId = context.ClientId,
      Scope = context.Scope,
      CodeChallenge = context.CodeChallenge,
      CodeChallengeMethod = context.CodeChallengeMethod,
      MaxAge = context.MaxAge,
      Nonce = context.Nonce,
      RedirectUri = context.RedirectUri,
      ResponseType = context.ResponseType,
      State = context.State,
      UserId = userId
    };

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