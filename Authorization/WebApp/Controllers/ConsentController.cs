using Application;
using Domain;
using Infrastructure;
using Infrastructure.Decoders.Abstractions;
using Infrastructure.Helpers;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;
using Infrastructure.Requests.CreateOrUpdateConsentGrant;
using WebApp.Constants;
using WebApp.Extensions;
using WebApp.ViewModels;
using WebApp.Attributes;
using WebApp.Contracts;

namespace WebApp.Controllers;

[Route("connect/[controller]")]
public class ConsentController : OAuthControllerBase
{
  private readonly ICodeDecoder _codeDecoder;
  private readonly IdentityContext _identityContext;
  private readonly IMediator _mediator;

  public ConsentController(
    ICodeDecoder codeDecoder,
    IdentityContext identityContext,
    IMediator mediator,
    IdentityConfiguration identityConfiguration)
  : base (identityConfiguration)
  {
    _codeDecoder = codeDecoder;
    _identityContext = identityContext;
    _mediator = mediator;
  }

  [HttpGet]
  [SecurityHeader]
  public async Task<IActionResult> Index(
    [FromQuery(Name = ParameterNames.LoginCode)] string loginCode,
    CancellationToken cancellationToken = default)
  {
    var code = _codeDecoder.DecodeLoginCode(loginCode);
    var userToken = await _identityContext
      .Set<UserToken>()
      .SingleAsync(x => x.User.Id == code.UserId && x.Value == loginCode, cancellationToken: cancellationToken);

    if (userToken.ExpiresAt < DateTime.UtcNow)
    {
      return Unauthorized(new ErrorResponse(ErrorCode.LoginRequired, "login is required"));
    }
    var scopes = HttpContext.Request.Query[ParameterNames.Scope].ToString().Split(' ');
    var userId = code.UserId;
    var claims = ClaimsHelper.MapToClaims(scopes);
    var clientId = HttpContext.Request.Query[ParameterNames.ClientId].ToString();
    var client = await _identityContext
      .Set<Client>()
      .SingleAsync(x => x.Id == clientId, cancellationToken: cancellationToken);

    var user = await _identityContext.Set<User>().SingleAsync(x => x.Id == userId, cancellationToken: cancellationToken);
    return View(new ConsentViewModel
    {
      Claims = claims,
      LoginCode = loginCode,
      ClientName = client.Name,
      GivenName = user.FirstName,
      TosUri = client.TosUri,
      PolicyUri = client.PolicyUri
    });
  }

  [HttpPost]
  [Consumes("application/x-www-form-urlencoded")]
  [ValidateAntiForgeryToken]
  [SecurityHeader]
  public async Task<IActionResult> Post(
    [FromQuery(Name = ParameterNames.LoginCode)] string loginCode,
    CancellationToken cancellationToken = default)
  {
    var command = HttpContext.Request.Query.ToAuthorizationGrantCommand(loginCode);
    var consentResponse = await _mediator.Send(new CreateOrUpdateConsentGrantCommand
    {
      LoginCode = loginCode,
      ClientId = command.ClientId,
      ConsentedClaims = HttpContext.Request.Form.Keys.Where(x => x != AntiForgeryConstants.AntiForgeryField).ToList(),
      ConsentedScopes = command.Scopes
    }, cancellationToken: cancellationToken);

    if (consentResponse.IsError())
    {
      return BadOAuthResult(consentResponse.ErrorCode, consentResponse.ErrorDescription);
    }

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