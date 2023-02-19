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
using WebApp.Contracts;
using WebApp.Controllers.Abstracts;

namespace WebApp.Controllers;

[ApiController]
[Route("connect/[controller]")]
public class TokenController : OAuthControllerBase
{
  private readonly IMediator _mediator;

  public TokenController(IMediator mediator, IdentityConfiguration identityConfiguration) : base(identityConfiguration)
  {
    _mediator = mediator;
  }

  [HttpPost]
  [SecurityHeader]
  [Consumes(MimeTypeConstants.FormUrlEncoded)]
  [ProducesResponseType(typeof(PostTokenResponse), StatusCodes.Status200OK)]
  [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
  public async Task<IActionResult> Post(
    [FromForm] PostTokenRequest request,
    CancellationToken cancellationToken = default)
  {
    return request.GrantType switch
    {
      GrantTypeConstants.AuthorizationCode => await PostAuthorize(request, cancellationToken: cancellationToken),
      GrantTypeConstants.RefreshToken => await PostRefresh(request, cancellationToken: cancellationToken),
      GrantTypeConstants.ClientCredentials => await PostClientCredentials(request, cancellationToken: cancellationToken),
      _ => BadOAuthResult(ErrorCode.UnsupportedGrantType, "grant_type is unsupported")
    };
  }

  private async Task<IActionResult> PostRefresh(
    PostTokenRequest request,
    CancellationToken cancellationToken = default)
  {
    var command = new RedeemRefreshTokenGrantCommand
    {
      GrantType = request.GrantType,
      ClientId = request.ClientId,
      ClientSecret = request.ClientSecret,
      RefreshToken = request.RefreshToken
    };
    var response = await _mediator.Send(command, cancellationToken: cancellationToken);
    if (response.IsError())
    {
      return BadOAuthResult(response.ErrorCode!, response.ErrorDescription!);
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
      PostTokenRequest request,
      CancellationToken cancellationToken = default)
  {
    var command = new RedeemAuthorizationCodeGrantCommand
    {
      GrantType = request.GrantType,
      ClientId = request.ClientId,
      ClientSecret = request.ClientSecret,
      RedirectUri = request.RedirectUri,
      Scope = request.Scope,
      CodeVerifier = request.CodeVerifier,
      Code = request.Code
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
    PostTokenRequest request,
    CancellationToken cancellationToken = default)
  {
    var command = new RedeemClientCredentialsGrantCommand
    {
      GrantType = request.GrantType,
      ClientId = request.ClientId,
      ClientSecret = request.ClientSecret,
      Scope = request.Scope
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