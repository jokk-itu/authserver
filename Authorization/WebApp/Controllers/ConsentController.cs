using System.IdentityModel.Tokens.Jwt;
using Application;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using Domain.Constants;
using Infrastructure.Decoders.Token;
using Infrastructure.Decoders.Token.Abstractions;
using Infrastructure.Requests.CreateAuthorizationGrant;
using Infrastructure.Requests.CreateOrUpdateConsentGrant;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using WebApp.Constants;
using WebApp.ViewModels;
using WebApp.Attributes;
using Infrastructure.Requests.GetConsentModel;
using WebApp.Controllers.Abstracts;
using WebApp.Context.Abstract;
using WebApp.Context.AuthorizeContext;

namespace WebApp.Controllers;

[Route("connect/[controller]")]
public class ConsentController : OAuthControllerBase
{
  private readonly IMediator _mediator;
  private readonly IContextAccessor<AuthorizeContext> _contextAccessor;
  private readonly IStructuredTokenDecoder _tokenDecoder;

  public ConsentController(
    IMediator mediator,
    IdentityConfiguration identityConfiguration,
    IContextAccessor<AuthorizeContext> contextAccessor,
    IStructuredTokenDecoder tokenDecoder)
  : base (identityConfiguration)
  {
    _mediator = mediator;
    _contextAccessor = contextAccessor;
    _tokenDecoder = tokenDecoder;
  }

  [HttpGet("create")]
  [SecurityHeader]
  [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
  public async Task<IActionResult> CreateConsent(CancellationToken cancellationToken = default)
  {
    var context = await _contextAccessor.GetContext(HttpContext);
    var userId = HttpContext.User.Claims.Single(x => x.Type == ClaimNameConstants.Sub).Value;
    return await GetConsentView(context, userId, HttpMethod.Post.Method, cancellationToken: cancellationToken);
  }

  [HttpGet("update")]
  [SecurityHeader]
  public async Task<IActionResult> UpdateConsent(CancellationToken cancellationToken = default)
  {
    var context = await _contextAccessor.GetContext(HttpContext);
    JwtSecurityToken token;
    try
    {
      token = await _tokenDecoder.Decode(context.IdTokenHint, new StructuredTokenDecoderArguments
      {
        ClientId = context.ClientId,
        Audiences = new [] { context.ClientId },
        ValidateAudience = true,
        ValidateLifetime = true
      });
    }
    catch
    {
      return ErrorFormPostResult(context.RedirectUri, context.State, ErrorCode.LoginRequired, "login is required");
    }

    var userId = token.Claims.Single(x => x.Type == ClaimNameConstants.Sub).Value;
    return await GetConsentView(context, userId, HttpMethod.Put.Method, cancellationToken: cancellationToken);
  }

  [HttpPost("create")]
  [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
  [Consumes(MimeTypeConstants.FormUrlEncoded)]
  [ValidateAntiForgeryToken]
  [SecurityHeader]
  public async Task<IActionResult> Post(CancellationToken cancellationToken = default)
  {
    await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

    var context = await _contextAccessor.GetContext(HttpContext);
    var userId = HttpContext.User.Claims.Single(x => x.Type == ClaimNameConstants.Sub).Value;
    var consentResponse = await _mediator.Send(new CreateOrUpdateConsentGrantCommand
    {
      UserId = userId,
      ClientId = context.ClientId,
      ConsentedClaims = ConsentHelper.GetConsentedClaims(HttpContext.Request.Form).ToList(),
      ConsentedScopes = context.Scope.Split(' ')
    }, cancellationToken: cancellationToken);

    if (consentResponse.IsError())
    {
      return BadOAuthResult(consentResponse.ErrorCode, consentResponse.ErrorDescription);
    }

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
  [ValidateAntiForgeryToken]
  [Consumes(MimeTypeConstants.FormUrlEncoded)]
  [SecurityHeader]
  public async Task<IActionResult> Put(CancellationToken cancellationToken = default)
  {
    var context = await _contextAccessor.GetContext(HttpContext);
    JwtSecurityToken token;
    try
    {
      token = await _tokenDecoder.Decode(context.IdTokenHint, new StructuredTokenDecoderArguments
      {
        ClientId = context.ClientId,
        Audiences = new [] { context.ClientId },
        ValidateAudience = true,
        ValidateLifetime = true
      });
    }
    catch
    {
      return BadOAuthResult(ErrorCode.LoginRequired, "login is required");
    }

    var consentResponse = await _mediator.Send(new CreateOrUpdateConsentGrantCommand
    {
      UserId = token.Claims.Single(x => x.Type == ClaimNameConstants.Sub).Value,
      ClientId = context.ClientId,
      ConsentedClaims = ConsentHelper.GetConsentedClaims(HttpContext.Request.Form).ToList(),
      ConsentedScopes = context.Scope.Split(' ')
    }, cancellationToken: cancellationToken);

    if (consentResponse.IsError())
    {
      return BadOAuthResult(consentResponse.ErrorCode, consentResponse.ErrorDescription);
    }

    return Ok();
  }

  private async Task<IActionResult> GetConsentView(AuthorizeContext context, string userId, string method, CancellationToken cancellationToken = default)
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

    return View("Index", new ConsentViewModel
    {
      Claims = response.Claims,
      Scopes = response.Scopes,
      ClientName = response.ClientName,
      GivenName = response.GivenName,
      PolicyUri = response.PolicyUri, 
      TosUri = response.TosUri,
      LogoUri = response.LogoUri,
      FormMethod = method
    });
  }
}