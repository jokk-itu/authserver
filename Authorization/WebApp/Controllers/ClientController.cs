using Contracts;
using Infrastructure.Helpers;
using Infrastructure.Requests.CreateClient;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.Contracts.GetClient;
using WebApp.Contracts.PostClient;
using WebApp.Contracts.PutClient;

namespace WebApp.Controllers;


[Route("/connect/[controller]")]
public class ClientController : Controller
{
  private readonly IMediator _mediator;

  public ClientController(IMediator mediator)
  {
    _mediator = mediator;
  }

  [HttpPost]
  [Authorize(Policy = "InitialAccessToken")]
  [Route("register")]
  [ProducesResponseType(typeof(PostClientResponse), StatusCodes.Status201Created)]
  [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
  public async Task<IActionResult> PostClientAsync([FromBody] PostClientRequest request, CancellationToken cancellationToken = default)
  {
    // TODO validate request
    var scopes = request.Scope.Split(' ');
    var response = await _mediator.Send(new CreateClientCommand
    {
      ClientName = request.ClientName,
      ApplicationType = request.ApplicationType,
      GrantTypes = request.GrantTypes,
      ResponseTypes = request.ResponseTypes,
      TokenEndpointAuthMethod = request.TokenEndpointAuthMethod,
      TosUri = request.TosUri,
      Contacts = request.Contacts,
      RedirectUris = request.RedirectUris,
      PolicyUri = request.PolicyUri,
      SubjectType = request.SubjectType,
      Scopes = scopes
    }, cancellationToken: cancellationToken);

    if (response.IsError())
      return BadRequest();

    var uri = $"{Request.Scheme}://{Request.Host}/connect/client/configuration";
    return Created(new Uri(uri), new PostClientResponse
    {
      ApplicationType = response.ApplicationType,
      GrantTypes = response.GrantTypes,
      ResponseTypes = response.ResponseTypes,
      TokenEndpointAuthMethod = response.TokenEndpointAuthMethod,
      ClientName = response.ClientName,
      TosUri = response.TosUri,
      ClientId = response.ClientId,
      ClientSecret = response.ClientSecret,
      Contacts = response.Contacts,
      PolicyUri = response.PolicyUri,
      RedirectUris = response.RedirectUris,
      RegistrationAccessToken = response.RegistrationAccessToken,
      RegistrationClientUri = uri,
      Scope = response.Scope,
      SubjectType = response.SubjectType,
      ClientSecretExpiresAt = 0
    });
  }

  [HttpPut]
  [Authorize(Policy = "RegistrationAccessToken")]
  [Route("configuration")]
  [ProducesResponseType(typeof(PutClientResponse), StatusCodes.Status200OK)]
  public async Task<IActionResult> PutClientAsync([FromBody] PutClientRequest request)
  {
    return Ok();
  }

  [HttpDelete]
  [Authorize(Policy = "RegistrationAccessToken")]
  [Route("configuration")]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  public async Task<IActionResult> DeleteClientAsync()
  {
    return NoContent();
  }

  [HttpGet]
  [Authorize(Policy = "RegistrationAccessToken")]
  [Route("configuration")]
  [ProducesResponseType(typeof(GetClientResponse), StatusCodes.Status200OK)]
  public async Task<IActionResult> GetClientAsync()
  {
    return Ok();
  }
}