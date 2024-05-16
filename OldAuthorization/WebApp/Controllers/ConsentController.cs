using Application;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using Domain.Constants;
using Infrastructure.Requests.CreateAuthorizationGrant;
using Infrastructure.Requests.CreateConsentGrant;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using WebApp.Constants;
using WebApp.ViewModels;
using WebApp.Attributes;
using Infrastructure.Requests.GetConsentModel;
using Infrastructure.Services.Abstract;
using Mapster;
using WebApp.Controllers.Abstracts;
using WebApp.Context.Abstract;
using WebApp.Context.AuthorizeContext;

namespace WebApp.Controllers;

[Route("connect/[controller]")]
public class ConsentController : OAuthControllerBase
{
  private readonly IMediator _mediator;
  private readonly IContextAccessor<AuthorizeContext> _contextAccessor;
  private readonly IClientService _clientService;

  public ConsentController(
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
  [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
  public async Task<IActionResult> Index(CancellationToken cancellationToken)
  {
    var context = await _contextAccessor.GetContext(HttpContext);
    var clientValidation = await _clientService.ValidateRedirectAuthorization(
      context.ClientId, context.RedirectUri, context.State, cancellationToken);

    if (clientValidation.IsError())
    {
      return BadOAuthResult(clientValidation.ErrorCode, clientValidation.ErrorDescription);
    }

    var userId = HttpContext.User.Claims.Single(x => x.Type == ClaimNameConstants.Sub).Value;
    return await GetConsentView(context, userId, cancellationToken: cancellationToken);
  }

  [HttpPost]
  [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
  [Consumes(MimeTypeConstants.FormUrlEncoded)]
  [ValidateAntiForgeryToken]
  [SecurityHeader]
  public async Task<IActionResult> Post(CancellationToken cancellationToken)
  {
    var context = await _contextAccessor.GetContext(HttpContext);
    var clientValidation = await _clientService.ValidateRedirectAuthorization(
      context.ClientId, context.RedirectUri, context.State, cancellationToken);

    if (clientValidation.IsError())
    {
      return BadOAuthResult(clientValidation.ErrorCode, clientValidation.ErrorDescription);
    }

    var userId = HttpContext.User.Claims.Single(x => x.Type == ClaimNameConstants.Sub).Value;
    var consentResponse = await _mediator.Send(new CreateConsentGrantCommand
    {
      UserId = userId,
      ClientId = context.ClientId,
      ConsentedClaims = FormHelper.GetFilteredKeys(HttpContext.Request.Form).ToList(),
      ConsentedScopes = context.Scope.Split(' ')
    }, cancellationToken: cancellationToken);

    if (consentResponse.IsError())
    {
      return BadOAuthResult(consentResponse.ErrorCode, consentResponse.ErrorDescription);
    }

    var command = context.Adapt<CreateAuthorizationGrantCommand>();
    command.UserId = userId;
    var response = await _mediator.Send(command, cancellationToken: cancellationToken);
    return response.StatusCode switch
    {
      HttpStatusCode.OK when response.IsError() =>
        ErrorFormPostResult(command.RedirectUri, command.State, response.ErrorCode, response.ErrorDescription),
      HttpStatusCode.BadRequest when response.IsError() =>
        BadOAuthResult(response.ErrorCode!, response.ErrorDescription!),
      HttpStatusCode.OK => AuthorizationCodeFormPostResult(command.RedirectUri, response.State, response.Code),
      _ => BadOAuthResult(ErrorCode.ServerError, "something went wrong")
    };
  }

  private async Task<IActionResult> GetConsentView(AuthorizeContext context, string userId,
    CancellationToken cancellationToken)
  {
    var query = new GetConsentModelQuery
    {
      Scope = context.Scope,
      ClientId = context.ClientId,
      UserId = userId
    };
    var response = await _mediator.Send(query, cancellationToken: cancellationToken);

    if (response.IsError())
    {
      return ErrorFormPostResult(context.RedirectUri, context.State, response.ErrorCode, response.ErrorDescription);
    }

    return View("Index", response.Adapt<ConsentViewModel>());
  }
}