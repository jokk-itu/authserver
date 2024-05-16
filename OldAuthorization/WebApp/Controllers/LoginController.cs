using System.Net;
using System.Security.Claims;
using Application;
using Domain.Constants;
using Infrastructure.Requests.CreateAuthorizationGrant;
using Infrastructure.Requests.Login;
using Infrastructure.Services.Abstract;
using Mapster;
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
using WebApp.ViewModels;

namespace WebApp.Controllers;

[Route("connect/[controller]")]
public class LoginController : OAuthControllerBase
{
  private readonly IMediator _mediator;
  private readonly IContextAccessor<AuthorizeContext> _contextAccessor;
  private readonly IClientService _clientService;

  public LoginController(
    IMediator mediator,
    IdentityConfiguration identityConfiguration,
    IContextAccessor<AuthorizeContext> contextAccessor,
    IClientService clientService)
    : base(identityConfiguration)
  {
    _mediator = mediator;
    _contextAccessor = contextAccessor;
    _clientService = clientService;
  }

  [HttpGet]
  [SecurityHeader]
  public async Task<IActionResult> Index()
  {
    var authorizeContext = await _contextAccessor.GetContext(HttpContext);
    return View(new LoginViewModel
    {
      LoginHint = authorizeContext.LoginHint
    });
  }

  [HttpPost]
  [ValidateAntiForgeryToken]
  [SecurityHeader]
  [Consumes(MimeTypeConstants.FormUrlEncoded)]
  [ProducesResponseType(StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  public async Task<IActionResult> Post(
    PostLoginRequest request,
    CancellationToken cancellationToken)
  {
    var context = await _contextAccessor.GetContext(HttpContext);
    var clientValidation = await _clientService.ValidateRedirectAuthorization(
      context.ClientId, context.RedirectUri, context.State, cancellationToken);

    if (clientValidation.IsError())
    {
      return BadOAuthResult(clientValidation.ErrorCode, clientValidation.ErrorDescription);
    }

    var query = new LoginQuery
    {
      Username = request.Username,
      Password = request.Password
    };
    var loginResponse = await _mediator.Send(query, cancellationToken: cancellationToken);

    if (loginResponse.IsError())
    {
      // TODO use ModelState.AddError because login is not OIDC constrained, we do not need to return to client
      return BadOAuthResult(loginResponse.ErrorCode, loginResponse.ErrorDescription);
    }

    var claims = new[]
    {
      new Claim(ClaimNameConstants.Sub, loginResponse.UserId),
      new Claim(ClaimNameConstants.Name, loginResponse.Name)
    };
    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
    await HttpContext.SignInAsync(
      CookieAuthenticationDefaults.AuthenticationScheme,
      new ClaimsPrincipal(identity));

    var routeValues = HttpContext.Request.Query.ToRouteValueDictionary();
    if (string.IsNullOrWhiteSpace(context.Prompt))
    {
      var isConsentValid = await _clientService.IsConsentValid(context.ClientId, loginResponse.UserId, context.Scope, cancellationToken);
      if (isConsentValid)
      {
        return await GetAuthorizationCode(context, loginResponse.UserId, cancellationToken: cancellationToken);
      }

      return RedirectToAction(controllerName: "Consent", actionName: "Index", routeValues: routeValues);
    }
    
    var prompts = context.Prompt.Split(' ');
    if (!prompts.Contains(PromptConstants.Consent))
    {
      return await GetAuthorizationCode(context, loginResponse.UserId, cancellationToken: cancellationToken);
    }

    return RedirectToAction(controllerName: "Consent", actionName: "Index", routeValues: routeValues);
  }

  private async Task<IActionResult> GetAuthorizationCode(AuthorizeContext context, string userId,
    CancellationToken cancellationToken)
  {
    var command = context.Adapt<CreateAuthorizationGrantCommand>();
    command.UserId = userId;

    var authorizationGrantResponse = await _mediator.Send(command, cancellationToken: cancellationToken);
    return authorizationGrantResponse.StatusCode switch
    {
      HttpStatusCode.OK when authorizationGrantResponse.IsError() =>
        ErrorFormPostResult(command.RedirectUri, command.State, authorizationGrantResponse.ErrorCode,
          authorizationGrantResponse.ErrorDescription),
      HttpStatusCode.BadRequest when authorizationGrantResponse.IsError() =>
        BadOAuthResult(authorizationGrantResponse.ErrorCode, authorizationGrantResponse.ErrorDescription),
      HttpStatusCode.OK => AuthorizationCodeFormPostResult(command.RedirectUri, authorizationGrantResponse.State,
        authorizationGrantResponse.Code),
      _ => BadOAuthResult(ErrorCode.ServerError, "unexpected error occurred")
    };
  }
}