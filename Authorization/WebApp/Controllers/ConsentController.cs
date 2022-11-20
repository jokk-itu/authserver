using Application;
using Domain;
using Domain.Constants;
using Infrastructure;
using Infrastructure.Decoders.Abstractions;
using Infrastructure.Helpers;
using Infrastructure.Requests.CreateAuthorizationGrant;
using MediatR;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;
using WebApp.Constants;
using WebApp.Extensions;
using WebApp.ViewModels;

namespace WebApp.Controllers;

[Route("connect/[controller]")]
public class ConsentController : Controller
{
  private readonly ITokenDecoder _tokenDecoder;
  private readonly IdentityContext _identityContext;
  private readonly UserManager<User> _userManager;
  private readonly IMediator _mediator;

  public ConsentController(
    ITokenDecoder tokenDecoder,
    IdentityContext identityContext,
    UserManager<User> userManager,
    IMediator mediator)
  {
    _tokenDecoder = tokenDecoder;
    _identityContext = identityContext;
    _userManager = userManager;
    _mediator = mediator;
  }

  [HttpGet]
  public async Task<IActionResult> Index(
    [FromQuery(Name = ParameterNames.LoginToken)] string loginToken,
    CancellationToken cancellationToken = default)
  {
    var decryptedToken = _tokenDecoder.DecodeEncryptedToken(loginToken);
    var scopes = HttpContext.Request.Query[ParameterNames.Scope].ToString().Split(' ');
    var userId = decryptedToken!.Claims.Single(x => x.Type == ClaimNameConstants.Sub).Value;
    var claims = ClaimsHelper.MapToClaims(scopes);
    var clientId = HttpContext.Request.Query[ParameterNames.ClientId].ToString();
    var client = await _identityContext
      .Set<Client>()
      .SingleAsync(x => x.Id == clientId, cancellationToken: cancellationToken);

    var user = await _userManager.FindByIdAsync(userId);
    return View(new ConsentViewModel
    {
      Claims = claims,
      LoginToken = loginToken,
      ClientName = client.Name,
      GivenName = user.FirstName,
      TosUri = client.TosUri,
      PolicyUri = client.PolicyUri
    });
  }

  [HttpPost]
  [Consumes("application/x-www-form-urlencoded")]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> Post(
    [FromQuery(Name = ParameterNames.LoginToken)] string loginToken,
    CancellationToken cancellationToken = default)
  {
    var command = HttpContext.Request.Query.ToAuthorizationGrantCommand(loginToken);
    var decryptedToken = _tokenDecoder.DecodeEncryptedToken(loginToken);
    var userId = decryptedToken!.Claims.Single(x => x.Type == ClaimNameConstants.Sub).Value;
    var consentGrant = await _identityContext
      .Set<ConsentGrant>()
      .Include(x => x.ConsentedClaims)
      .Include(x => x.ConsentedScopes)
      .Where(x => x.Client.Id == command.ClientId)
      .Where(x => x.User.Id == userId)
      .SingleOrDefaultAsync(cancellationToken: cancellationToken);

    var claims = await _identityContext
      .Set<Claim>()
      .Where(x => HttpContext.Request.Form.Keys.Any(y => y == x.Name))
      .ToListAsync(cancellationToken: cancellationToken);

    var scopes = await _identityContext
      .Set<Scope>()
      .Where(x => command.Scopes.Any(y => y == x.Name))
      .ToListAsync(cancellationToken: cancellationToken);

    if (consentGrant is not null)
    {
      consentGrant.Updated = DateTime.UtcNow;
      consentGrant.ConsentedClaims = claims;
      consentGrant.ConsentedScopes = scopes;
    }
    else
    {
      consentGrant = new ConsentGrant
      {
        Client = await _identityContext
          .Set<Client>()
          .SingleAsync(cancellationToken: cancellationToken),
        User = await _userManager.FindByIdAsync(userId),
        ConsentedClaims = claims,
        ConsentedScopes = scopes,
        IssuedAt = DateTime.UtcNow,
        Updated = DateTime.UtcNow
      };
      await _identityContext
        .Set<ConsentGrant>()
        .AddAsync(consentGrant, cancellationToken);
    }
    await _identityContext.SaveChangesAsync(cancellationToken);

    var response = await _mediator.Send(command, cancellationToken: cancellationToken);
    return response.StatusCode switch
    {
      HttpStatusCode.Redirect when response.IsError() => 
        this.RedirectOAuthResult(command.RedirectUri, command.State, response.ErrorCode!, response.ErrorDescription!),
      HttpStatusCode.BadRequest when response.IsError() =>
        this.BadOAuthResult(response.ErrorCode!, response.ErrorDescription!),
      HttpStatusCode.Redirect => Redirect($"{command.RedirectUri}{GetCodeQuery(response)}"),
      _ => this.BadOAuthResult(ErrorCode.ServerError, "something went wrong")
    };
  }

  private static QueryString GetCodeQuery(CreateAuthorizationGrantResponse response)
  {
    return new QueryBuilder
    {
      {ParameterNames.State, response.State},
      {ParameterNames.Code, response.Code}
    }.ToQueryString();
  }
}
