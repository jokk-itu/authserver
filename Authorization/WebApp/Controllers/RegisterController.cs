using Application;
using Domain.Constants;
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
using WebApp.Context.Abstract;
using WebApp.Context.ClientContext;
using WebApp.Contracts;
using WebApp.Controllers.Abstracts;

namespace WebApp.Controllers;

[Route("/connect/[controller]")]
public class RegisterController : OAuthControllerBase
{
  private readonly IMediator _mediator;
  private readonly IContextAccessor<ClientContext> _contextAccessor;

  public RegisterController(
    IMediator mediator,
    IdentityConfiguration identityConfiguration,
    IContextAccessor<ClientContext> contextAccessor) : base(identityConfiguration)
  {
    _mediator = mediator;
    _contextAccessor = contextAccessor;
  }

  [HttpPost]
  [ProducesResponseType(typeof(ClientResponse), StatusCodes.Status201Created)]
  [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
  public async Task<IActionResult> Post(CancellationToken cancellationToken = default)
  {
    var context = await _contextAccessor.GetContext(HttpContext);
    var command = context.Adapt<CreateClientCommand>();
    var response = await _mediator.Send(command, cancellationToken: cancellationToken);

    if (response.IsError())
    {
      return BadOAuthResult(response.ErrorCode, response.ErrorDescription);
    }

    var createdResponse = response.Adapt<ClientResponse>();
    return CreatedOAuthResult($"connect/register?{response.ClientId}", createdResponse);
  }

  [HttpPut("{clientId}")]
  [Authorize(Policy = AuthorizationConstants.ClientConfiguration,
    AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
  [ProducesResponseType(StatusCodes.Status200OK)]
  [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
  public async Task<IActionResult> Put(string clientId, CancellationToken cancellationToken = default)
  {
    var context = await _contextAccessor.GetContext(HttpContext);
    return Ok();
  }

  [HttpDelete("{clientId}")]
  [Authorize(Policy = AuthorizationConstants.ClientConfiguration, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
  public async Task<IActionResult> Delete(string clientId, CancellationToken cancellationToken = default)
  {
    var token = await HttpContext.GetTokenAsync(JwtBearerDefaults.AuthenticationScheme, TokenTypeConstants.AccessToken)
      ?? string.Empty;
    var command = new DeleteClientCommand(clientId, token);
    var response = await _mediator.Send(command, cancellationToken: cancellationToken);

    if (response.IsError())
    {
      return BadOAuthResult(response.ErrorCode, response.ErrorDescription);
    }

    return NoContent();
  }

  [HttpGet("{clientId}")]
  [Authorize(Policy = AuthorizationConstants.ClientConfiguration, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
  [ProducesResponseType(typeof(ClientResponse), StatusCodes.Status200OK)]
  [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
  public async Task<IActionResult> Get(string clientId, CancellationToken cancellationToken = default)
  {
    var token = await HttpContext.GetTokenAsync(JwtBearerDefaults.AuthenticationScheme, TokenTypeConstants.AccessToken)
      ?? string.Empty;
    var response = await _mediator.Send(new ReadClientQuery(clientId, token), cancellationToken);

    if (response.IsError())
    {
      return BadOAuthResult(response.ErrorCode, response.ErrorDescription);
    }

    return Ok(response.Adapt<ClientResponse>());
  }
}