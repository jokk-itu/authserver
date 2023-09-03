using System.Net;
using Application;
using Infrastructure.Requests.SilentCookieLogin;
using Infrastructure.Services.Abstract;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.Attributes;
using WebApp.Constants;
using WebApp.Context.Abstract;
using WebApp.Context.AuthorizeContext;
using WebApp.Controllers.Abstracts;
using WebApp.Extensions;

namespace WebApp.Controllers;

[Route("connect/[controller]")]
public class SelectAccountController : OAuthControllerBase
{
  private readonly IMediator _mediator;
  private readonly IContextAccessor<AuthorizeContext> _contextAccessor;
  private readonly IClientService _clientService;

  public SelectAccountController(
    IdentityConfiguration identityConfiguration,
    IMediator mediator,
    IContextAccessor<AuthorizeContext> contextAccessor,
    IClientService clientService)
    : base(identityConfiguration)
  {
    _mediator = mediator;
    _contextAccessor = contextAccessor;
    _clientService = clientService;
  }

  [HttpGet]
  [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
  [SecurityHeader]
  public IActionResult Index()
  {
    return View("Index");
  }

  [HttpPost]
  [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
  [Consumes(MimeTypeConstants.FormUrlEncoded)]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> Post(CancellationToken cancellationToken = default)
  {
    var context = await _contextAccessor.GetContext(HttpContext);
    var clientValidation = await _clientService
      .ValidateRedirectAuthorization(context.ClientId, context.RedirectUri, context.State, cancellationToken);

    if (clientValidation.IsError())
    {
      return BadOAuthResult(clientValidation.ErrorCode, clientValidation.ErrorDescription);
    }

    var command = context.Adapt<SilentCookieLoginCommand>();
    command.UserId = FormHelper.GetFilteredKeys(HttpContext.Request.Form).Single(); 
    var response = await _mediator.Send(command, cancellationToken);

    if (response.IsError() && string.IsNullOrWhiteSpace(context.Prompt))
    {
      var routeValues = HttpContext.Request.Query.ToRouteValueDictionary();
      return RedirectToAction(controllerName: "Login", actionName: "Index", routeValues: routeValues);
    }

    if (response.IsError())
    {
      if (response.StatusCode == HttpStatusCode.OK)
      {
        return ErrorFormPostResult(context.RedirectUri, context.State, response.ErrorCode, response.ErrorDescription);
      }

      return BadOAuthResult(response.ErrorCode, response.ErrorDescription);
    }

    return AuthorizationCodeFormPostResult(
      context.RedirectUri,
      context.State,
      response.Code);
  }
}