using Application;
using Infrastructure.Requests.TokenIntrospection;
using MediatR;
using WebApp.Controllers.Abstracts;
using Microsoft.AspNetCore.Mvc;
using WebApp.Context.Abstract;
using WebApp.Context.IntrospectionContext;
using WebApp.Contracts.PostIntrospection;

namespace WebApp.Controllers;

[Route("connect/token/[controller]")]
public class IntrospectionController : OAuthControllerBase
{
  private readonly IMediator _mediator;
  private readonly IContextAccessor<IntrospectionContext> _contextAccessor;

  public IntrospectionController(
    IdentityConfiguration identityConfiguration,
    IMediator mediator,
    IContextAccessor<IntrospectionContext> contextAccessor)
    : base(identityConfiguration)
  {
    _mediator = mediator;
    _contextAccessor = contextAccessor;
  }

  [HttpPost]
  [ProducesResponseType(typeof(PostIntrospectionResponse), StatusCodes.Status200OK)]
  public async Task<IActionResult> Post(CancellationToken cancellationToken = default)
  {
    var context = await _contextAccessor.GetContext(HttpContext);
    var query = new TokenIntrospectionQuery
    {
      Token = context.Token,
      TokenTypeHint = context.TokenTypeHint,
      ClientId = context.ClientId,
      ClientSecret = context.ClientSecret
    };

    var response = await _mediator.Send(query, cancellationToken: cancellationToken);
    if (response.IsError())
    {
      return BadOAuthResult(response.ErrorCode, response.ErrorDescription);
    }

    return Ok(new PostIntrospectionResponse
    {
      Active = response.Active,
      ClientId = response.ClientId,
      Issuer = response.Issuer,
      Username = response.UserName,
      TokenType = response.TokenType,
      Audience = response.Audience,
      ExpiresAt = response.ExpiresAt,
      IssuedAt = response.IssuedAt,
      JwtId = response.JwtId,
      NotBefore = response.NotBefore,
      Scope = response.Scope,
      Subject = response.Subject
    });
  }
}
