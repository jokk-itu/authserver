using Application;
using MediatR;
using Microsoft.AspNetCore.Mvc;
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
using Infrastructure.Decoders.Abstractions;
using Infrastructure.Requests.GetConsentModel;
using WebApp.Controllers.Abstracts;

namespace WebApp.Controllers;

[Route("connect/[controller]")]
public class ConsentController : OAuthControllerBase
{
  private readonly IMediator _mediator;
  private readonly ITokenDecoder _tokenDecoder;

  public ConsentController(
    IMediator mediator,
    IdentityConfiguration identityConfiguration,
    ITokenDecoder tokenDecoder)
  : base (identityConfiguration)
  {
    _mediator = mediator;
    _tokenDecoder = tokenDecoder;
  }

  [HttpGet("create")]
  [SecurityHeader]
  [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
  public async Task<IActionResult> CreateConsent(
    AuthorizeRequest request,
    CancellationToken cancellationToken = default)
  {
    return await GetConsentView(request, HttpContext.User.FindFirst(ClaimNameConstants.Sub)!.Value, HttpMethod.Post.Method, cancellationToken: cancellationToken);
  }

  [HttpGet("update")]
  [SecurityHeader]
  public async Task<IActionResult> UpdateConsent(
    AuthorizeRequest request,
    CancellationToken cancellationToken = default)
  {
    var token = _tokenDecoder.DecodeSignedToken(request.IdTokenHint);
    if (token is null)
    {
      return ErrorFormPostResult(request.RedirectUri, request.State, ErrorCode.LoginRequired, "login is required");
    }

    return await GetConsentView(request, token.Claims.First(x => x.Type == ClaimNameConstants.Sub)!.Value, HttpMethod.Put.Method, cancellationToken: cancellationToken);
  }

  [HttpPost("create")]
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

  [HttpPut("update")]
  [Consumes(MimeTypeConstants.FormUrlEncoded)]
  [ValidateAntiForgeryToken]
  [SecurityHeader]
  public async Task<IActionResult> Put(
    AuthorizeRequest request,
    CancellationToken cancellationToken = default)
  {
    var token = _tokenDecoder.DecodeSignedToken(request.IdTokenHint);
    if (token is null)
    {
      return BadOAuthResult(ErrorCode.LoginRequired, "login is required");
    }

    var consentResponse = await _mediator.Send(new CreateOrUpdateConsentGrantCommand
    {
      UserId = token.Claims.First(x => x.Type == ClaimNameConstants.Sub)!.Value,
      ClientId = request.ClientId,
      ConsentedClaims = ConsentHelper.GetConsentedClaims(HttpContext.Request.Form).ToList(),
      ConsentedScopes = request.Scope.Split(' ')
    }, cancellationToken: cancellationToken);

    if (consentResponse.IsError())
    {
      return BadOAuthResult(consentResponse.ErrorCode, consentResponse.ErrorDescription);
    }

    return Ok();
  }

  private async Task<IActionResult> GetConsentView(AuthorizeRequest request, string userId, string method, CancellationToken cancellationToken = default)
  {
    var query = new GetConsentModelQuery
    {
      Scope = request.Scope,
      ClientId = request.ClientId,
      UserId = userId
    };
    var response = await _mediator.Send(query, cancellationToken: cancellationToken);

    if (response.IsError())
    {
      return ErrorFormPostResult(request.RedirectUri, request.State, response.ErrorCode, response.ErrorDescription);
    }

    return View("Index", new ConsentViewModel
    {
      Claims = response.Claims,
      ClientName = response.ClientName,
      GivenName = response.GivenName,
      PolicyUri = response.PolicyUri,
      TosUri = response.TosUri,
      FormMethod = method
    });
  }
}