using Contracts;
using Infrastructure.Builders.Abstractions;
using Infrastructure.Requests.CreateClient;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.Constants;
using WebApp.Contracts.GetClient;
using WebApp.Contracts.GetResourceInitialAccessToken;
using WebApp.Contracts.PostClient;
using WebApp.Contracts.PutClient;
using WebApp.Extensions;

namespace WebApp.Controllers;


[Route("/connect/[controller]")]
public class ClientController : Controller
{
  private readonly IMediator _mediator;
  private readonly ITokenBuilder _tokenBuilder;

  public ClientController(IMediator mediator, ITokenBuilder tokenBuilder)
  {
    _mediator = mediator;
    _tokenBuilder = tokenBuilder;
  }

  [HttpGet]
  [Route("initial-token")]
  [AllowAnonymous]
  [ProducesResponseType(typeof(GetResourceInitialAccessToken), StatusCodes.Status200OK)]
  public IActionResult GetResourceInitialToken()
  {
    var token = _tokenBuilder.BuildClientInitialAccessToken();
    return Ok(new GetResourceInitialAccessToken
    {
      AccessToken = token,
      ExpiresIn = 300
    });
  }

  [HttpPost]
  [Authorize(Policy = AuthorizationConstants.ClientRegistration)]
  [Route("register")]
  [ProducesResponseType(typeof(PostClientResponse), StatusCodes.Status201Created)]
  [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
  public async Task<IActionResult> PostClientAsync([FromBody] PostClientRequest request, CancellationToken cancellationToken = default)
  {
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
      return this.BadOAuthResult(response.ErrorCode!, response.ErrorDescription!);

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
  [Authorize(Policy = AuthorizationConstants.ClientConfiguration)]
  [Route("configuration")]
  [ProducesResponseType(typeof(PutClientResponse), StatusCodes.Status200OK)]
  public async Task<IActionResult> PutClientAsync([FromBody] PutClientRequest request, CancellationToken cancellationToken = default)
  {
    return Ok();
  }

  [HttpDelete]
  [Authorize(Policy = AuthorizationConstants.ClientConfiguration)]
  [Route("configuration")]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  public async Task<IActionResult> DeleteClientAsync(CancellationToken cancellationToken = default)
  {
    return NoContent();
  }

  [HttpGet]
  [Authorize(Policy = AuthorizationConstants.ClientConfiguration)]
  [Route("configuration")]
  [ProducesResponseType(typeof(GetClientResponse), StatusCodes.Status200OK)]
  public async Task<IActionResult> GetClientAsync(CancellationToken cancellationToken = default)
  {
    return Ok();
  }
}