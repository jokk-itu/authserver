using Application;
using Domain.Constants;
using Infrastructure.Builders.Abstractions;
using Infrastructure.Requests.CreateClient;
using Infrastructure.Requests.DeleteClient;
using Infrastructure.Requests.ReadClient;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.Constants;
using WebApp.Contracts;
using WebApp.Contracts.GetResourceInitialAccessToken;
using WebApp.Contracts.PostClient;
using WebApp.Controllers.Abstracts;
using GetClientResponse = WebApp.Contracts.GetClient.GetClientResponse;

namespace WebApp.Controllers;

[Route("/connect/[controller]")]
public class ClientController : OAuthControllerBase
{
  private readonly IMediator _mediator;
  private readonly ITokenBuilder _tokenBuilder;

  public ClientController(
    IMediator mediator,
    ITokenBuilder tokenBuilder,
    IdentityConfiguration identityConfiguration) : base(identityConfiguration)
  {
    _mediator = mediator;
    _tokenBuilder = tokenBuilder;
  }

  [HttpGet]
  [Route("initial-token")]
  [AllowAnonymous]
  [ProducesResponseType(typeof(GetResourceInitialAccessToken), StatusCodes.Status200OK)]
  public IActionResult GetClientInitialToken()
  {
    var token = _tokenBuilder.BuildClientInitialAccessToken();
    return Ok(new GetResourceInitialAccessToken
    {
      AccessToken = token,
      ExpiresIn = 300
    });
  }

  [HttpPost]
  [Authorize(Policy = AuthorizationConstants.ClientRegistration, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
  [Route("register")]
  [ProducesResponseType(typeof(PostClientResponse), StatusCodes.Status201Created)]
  [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
  public async Task<IActionResult> Post([FromBody] PostClientRequest request, CancellationToken cancellationToken = default)
  {
    var response = await _mediator.Send(request.Adapt<CreateClientCommand>(), cancellationToken: cancellationToken);

    if (response.IsError())
    {
      return BadOAuthResult(response.ErrorCode, response.ErrorDescription);
    }

    return CreatedOAuthResult("connect/client/configuration", response.Adapt<PostClientResponse>());
  }

  [HttpDelete]
  [Authorize(Policy = AuthorizationConstants.ClientConfiguration, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
  [Route("configuration")]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  public async Task<IActionResult> Delete(CancellationToken cancellationToken = default)
  {
    var token = await HttpContext.GetTokenAsync(JwtBearerDefaults.AuthenticationScheme, TokenTypeConstants.AccessToken);
    var command = new DeleteClientCommand
    {
      ClientRegistrationToken = token
    };
    var response = await _mediator.Send(command, cancellationToken: cancellationToken);

    if (response.IsError())
    {
      return BadOAuthResult(response.ErrorCode, response.ErrorDescription);
    }

    return NoContent();
  }

  [HttpGet]
  [Authorize(Policy = AuthorizationConstants.ClientConfiguration)]
  [Route("configuration")]
  [ProducesResponseType(typeof(GetClientResponse), StatusCodes.Status200OK)]
  public async Task<IActionResult> Get(CancellationToken cancellationToken = default)
  {
    var token = await HttpContext.GetTokenAsync(JwtBearerDefaults.AuthenticationScheme, TokenTypeConstants.AccessToken);
    var response = await _mediator.Send(new ReadClientQuery(token!), cancellationToken);

    if (response.IsError())
    {
      return BadOAuthResult(response.ErrorCode, response.ErrorDescription);
    }

    return Ok(new GetClientResponse
    {
      ApplicationType = response.ApplicationType,
      ResponseTypes = response.ResponseTypes,
      Scope = response.Scope,
      TokenEndpointAuthMethod = response.TokenEndpointAuthMethod,
      RedirectUris = response.RedirectUris,
      SubjectType = response.SubjectType,
      Contacts = response.Contacts,
      PolicyUri = response.PolicyUri,
      ClientId = response.ClientId,
      GrantTypes = response.GrantTypes,
      ClientName = response.ClientName,
      ClientSecret = response.ClientSecret,
      TosUri = response.TosUri
    });
  }
}