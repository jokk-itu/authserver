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
using System.Threading;
using Infrastructure.Decoders.Abstractions;

namespace WebApp.Controllers;

[Route("connect/[controller]")]
public class ConsentController : OAuthControllerBase
{
  private readonly IdentityContext _identityContext;
  private readonly IMediator _mediator;
  private readonly ITokenDecoder _tokenDecoder;

  public ConsentController(
    IdentityContext identityContext,
    IMediator mediator,
    IdentityConfiguration identityConfiguration,
    ITokenDecoder tokenDecoder)
  : base (identityConfiguration)
  {
    _identityContext = identityContext;
    _mediator = mediator;
    _tokenDecoder = tokenDecoder;
  }

  [HttpGet]
  [SecurityHeader]
  [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
  public async Task<IActionResult> GetConsentForAuthorizeCode(
    AuthorizeRequest request,
    CancellationToken cancellationToken = default)
  {
    var scopes = request.Scope.Split(' ');
    var userId = HttpContext.User.FindFirst(ClaimNameConstants.Sub)!.Value;
    var claims = ClaimsHelper.MapToClaims(scopes);
    var client = await _identityContext
      .Set<Client>()
      .SingleAsync(x => x.Id == request.ClientId, cancellationToken: cancellationToken);

    var user = await _identityContext
      .Set<User>()
      .SingleAsync(x => x.Id == userId, cancellationToken: cancellationToken);

    return View("Index", new ConsentViewModel
    {
      Claims = claims,
      ClientName = client.Name,
      GivenName = user.FirstName,
      TosUri = client.TosUri,
      PolicyUri = client.PolicyUri,
      FormMethod = "POST"
    });
  }

  [HttpGet]
  [SecurityHeader]
  public async Task<IActionResult> GetConsent(
    AuthorizeRequest request,
    CancellationToken cancellationToken = default)
  {
    var scopes = request.Scope.Split(' ');
    var token = _tokenDecoder.DecodeSignedToken(request.IdTokenHint);
    if (token is null)
    {
      return ErrorFormPostResult(request.RedirectUri, request.State, ErrorCode.LoginRequired, "login is required");
    }

    var userId = token.Claims.Single(x => x.Type == ClaimNameConstants.Sub).Value;
    var claims = ClaimsHelper.MapToClaims(scopes);
    var client = await _identityContext
      .Set<Client>()
      .SingleAsync(x => x.Id == request.ClientId, cancellationToken: cancellationToken);

    var user = await _identityContext
      .Set<User>()
      .SingleAsync(x => x.Id == userId, cancellationToken: cancellationToken);

    return View("Index", new ConsentViewModel
    {
      Claims = claims,
      ClientName = client.Name,
      GivenName = user.FirstName,
      TosUri = client.TosUri,
      PolicyUri = client.PolicyUri,
      FormMethod = "PUT"
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

  [HttpPut]
  [Consumes(MimeTypeConstants.FormUrlEncoded)]
  [ValidateAntiForgeryToken]
  [SecurityHeader]
  public async Task<IActionResult> Put(
    AuthorizeRequest request,
    CancellationToken cancellationToken = default)
  {
    throw new NotImplementedException();
  }
}