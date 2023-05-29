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
using WebApp.Contracts;
using WebApp.Contracts.PostClient;
using WebApp.Controllers.Abstracts;
using GetClientResponse = WebApp.Contracts.GetClient.GetClientResponse;

namespace WebApp.Controllers;
[Route("/connect/[controller]")]
public class RegisterController : OAuthControllerBase
{
  private readonly IMediator _mediator;

  public RegisterController(
    IMediator mediator,
    IdentityConfiguration identityConfiguration) : base(identityConfiguration)
  {
    _mediator = mediator;
  }

  [HttpPost]
  [ProducesResponseType(typeof(PostClientResponse), StatusCodes.Status201Created)]
  [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
  public async Task<IActionResult> Post([FromBody] PostClientRequest request, CancellationToken cancellationToken = default)
  {
    var command = request.Adapt<CreateClientCommand>();
    var response = await _mediator.Send(command, cancellationToken: cancellationToken);

    if (response.IsError())
    {
      return BadOAuthResult(response.ErrorCode, response.ErrorDescription);
    }

    var createdResponse = response.Adapt<PostClientResponse>();
    return CreatedOAuthResult($"connect/register?{response.ClientId}", createdResponse);
  }

  [HttpDelete("{clientId}")]
  [Authorize(Policy = AuthorizationConstants.ClientConfiguration, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  public async Task<IActionResult> Delete(string clientId, CancellationToken cancellationToken = default)
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

  [HttpGet("{clientId}")]
  [Authorize(Policy = AuthorizationConstants.ClientConfiguration)]
  [ProducesResponseType(typeof(GetClientResponse), StatusCodes.Status200OK)]
  public async Task<IActionResult> Get(string clientId, CancellationToken cancellationToken = default)
  {
    var token = await HttpContext.GetTokenAsync(JwtBearerDefaults.AuthenticationScheme, TokenTypeConstants.AccessToken);
    var response = await _mediator.Send(new ReadClientQuery(token), cancellationToken);

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