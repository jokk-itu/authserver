using Microsoft.AspNetCore.Mvc;
using Domain.Constants;
using Application;
using Infrastructure.Requests.RedeemAuthorizationCodeGrant;
using WebApp.Contracts.PostToken;
using Infrastructure.Requests.RedeemClientCredentialsGrant;
using Infrastructure.Requests.RedeemRefreshTokenGrant;
using MediatR;
using WebApp.Attributes;
using WebApp.Constants;
using WebApp.Context;
using WebApp.Contracts;
using WebApp.Controllers.Abstracts;

namespace WebApp.Controllers;

[ApiController]
[Route("connect/[controller]")]
public class TokenController : OAuthControllerBase
{
  private readonly IMediator _mediator;
  private readonly IContextAccessor<TokenContext> _contextAccessor;

  public TokenController(
    IMediator mediator,
    IdentityConfiguration identityConfiguration,
    IContextAccessor<TokenContext> contextAccessor) : base(identityConfiguration)
  {
    _mediator = mediator;
    _contextAccessor = contextAccessor;
  }

  [HttpPost]
  [SecurityHeader]
  [Consumes(MimeTypeConstants.FormUrlEncoded)]
  [ProducesResponseType(typeof(PostTokenResponse), StatusCodes.Status200OK)]
  [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
  public async Task<IActionResult> Post(CancellationToken cancellationToken = default)
  {
    var context = await _contextAccessor.GetContext(HttpContext);
    return context.GrantType switch
    {
      GrantTypeConstants.AuthorizationCode => await PostAuthorize(context, cancellationToken: cancellationToken),
      GrantTypeConstants.RefreshToken => await PostRefresh(context, cancellationToken: cancellationToken),
      GrantTypeConstants.ClientCredentials => await PostClientCredentials(context, cancellationToken: cancellationToken),
      _ => BadOAuthResult(ErrorCode.UnsupportedGrantType, "grant_type is unsupported")
    };
  }

  private async Task<IActionResult> PostRefresh(
    TokenContext context,
    CancellationToken cancellationToken = default)
  {
    var command = new RedeemRefreshTokenGrantCommand
    {
      GrantType = context.GrantType,
      ClientId = context.ClientId,
      ClientSecret = context.ClientSecret,
      RefreshToken = context.RefreshToken
    };
    var response = await _mediator.Send(command, cancellationToken: cancellationToken);
    if (response.IsError())
    {
      return BadOAuthResult(response.ErrorCode, response.ErrorDescription);
    }

    return Ok(new PostTokenResponse
    {
      AccessToken = response.AccessToken,
      RefreshToken = response.RefreshToken,
      IdToken = response.IdToken,
      ExpiresIn = response.ExpiresIn
    });
  }

  private async Task<IActionResult> PostAuthorize(
      TokenContext context,
      CancellationToken cancellationToken = default)
  {
    var command = new RedeemAuthorizationCodeGrantCommand
    {
      GrantType = context.GrantType,
      ClientId = context.ClientId,
      ClientSecret = context.ClientSecret,
      RedirectUri = context.RedirectUri,
      Scope = context.Scope,
      CodeVerifier = context.CodeVerifier,
      Code = context.Code
    };
    var response = await _mediator.Send(command, cancellationToken: cancellationToken);
    if (response.IsError())
    {
      return BadOAuthResult(response.ErrorCode, response.ErrorDescription);
    }

    return Ok(new PostTokenResponse
    {
      AccessToken = response.AccessToken,
      RefreshToken = response.RefreshToken,
      IdToken = response.IdToken,
      ExpiresIn = response.ExpiresIn
    });
  }

  private async Task<IActionResult> PostClientCredentials(
    TokenContext context,
    CancellationToken cancellationToken = default)
  {
    var command = new RedeemClientCredentialsGrantCommand
    {
      GrantType = context.GrantType,
      ClientId = context.ClientId,
      ClientSecret = context.ClientSecret,
      Scope = context.Scope
    };
    var response = await _mediator.Send(command, cancellationToken: cancellationToken);
    if (response.IsError())
    {
      return BadOAuthResult(response.ErrorCode, response.ErrorDescription);
    }

    return Ok(new PostTokenResponse
    {
      AccessToken = response.AccessToken,
      ExpiresIn = response.ExpiresIn
    });
  }
}