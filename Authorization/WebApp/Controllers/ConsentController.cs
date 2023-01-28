using Application;
using Domain;
using Infrastructure;
using Infrastructure.Helpers;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;
using Domain.Constants;
using Infrastructure.Requests.CreateAuthorizationGrant;
using Infrastructure.Requests.CreateOrUpdateConsentGrant;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using WebApp.Constants;
using WebApp.ViewModels;
using WebApp.Attributes;
using WebApp.Contracts;

namespace WebApp.Controllers;

[Route("connect/[controller]")]
public class ConsentController : OAuthControllerBase
{
  private readonly IdentityContext _identityContext;
  private readonly IMediator _mediator;

  public ConsentController(
    IdentityContext identityContext,
    IMediator mediator,
    IdentityConfiguration identityConfiguration)
  : base (identityConfiguration)
  {
    _identityContext = identityContext;
    _mediator = mediator;
  }

  [HttpGet]
  [SecurityHeader]
  [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
  public async Task<IActionResult> Index(
    [FromQuery(Name = ParameterNames.Scope)] string scope,
    [FromQuery(Name = ParameterNames.ClientId)] string clientId,
    CancellationToken cancellationToken = default)
  {
    var scopes = scope.Split(' ');
    var userId = HttpContext.User.FindFirst(ClaimNameConstants.Sub)!.Value;
    var claims = ClaimsHelper.MapToClaims(scopes);
    var client = await _identityContext
      .Set<Client>()
      .SingleAsync(x => x.Id == clientId, cancellationToken: cancellationToken);

    var user = await _identityContext.Set<User>().SingleAsync(x => x.Id == userId, cancellationToken: cancellationToken);
    return View(new ConsentViewModel
    {
      Claims = claims,
      ClientName = client.Name,
      GivenName = user.FirstName,
      TosUri = client.TosUri,
      PolicyUri = client.PolicyUri
    });
  }

  [HttpPost]
  [Consumes(MimeTypeConstants.FormUrlEncoded)]
  [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
  [ValidateAntiForgeryToken]
  [SecurityHeader]
  public async Task<IActionResult> Post(
    AuthorizeRequest request,
    CancellationToken cancellationToken = default)
  {
    await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

    var userId = HttpContext.User.FindFirst(ClaimNameConstants.Sub)!.Value;
    var consentResponse = await _mediator.Send(new CreateOrUpdateConsentGrantCommand
    {
      UserId = userId,
      ClientId = request.ClientId,
      ConsentedClaims = ConsentHelper.GetConsentedClaims(HttpContext.Request.Form).ToList(),
      ConsentedScopes = request.Scope.Split(' ')
    }, cancellationToken: cancellationToken);

    if (consentResponse.IsError())
    {
      return BadOAuthResult(consentResponse.ErrorCode, consentResponse.ErrorDescription);
    }

    var maxAge = 0L;
    if (long.TryParse(request.MaxAge, out var parsedMaxAge))
    {
      maxAge = parsedMaxAge;
    }

    var command = new CreateAuthorizationGrantCommand
    {
      ClientId = request.ClientId,
      Scope = request.Scope,
      CodeChallenge = request.CodeChallenge,
      CodeChallengeMethod = request.CodeChallengeMethod,
      MaxAge = maxAge,
      Nonce = request.Nonce,
      RedirectUri = request.RedirectUri,
      ResponseType = request.ResponseType,
      State = request.State,
      UserId = userId
    };
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
}