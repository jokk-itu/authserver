﻿using Infrastructure.Builders.Abstractions;
using Infrastructure.Requests.CreateScope;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.Constants;
using WebApp.Contracts.GetScopeInitialAccessToken;
using WebApp.Contracts.PostScope;
using WebApp.Extensions;

namespace WebApp.Controllers;

[Route("connect/[controller]")]
public class ScopeController : Controller
{
  private readonly IMediator _mediator;
  private readonly ITokenBuilder _tokenBuilder;

  public ScopeController(IMediator mediator, ITokenBuilder tokenBuilder)
  {
    _mediator = mediator;
    _tokenBuilder = tokenBuilder;
  }

  [HttpGet]
  [Route("initial-token")]
  [ProducesResponseType(typeof(GetScopeInitialAccessToken), StatusCodes.Status200OK)]
  public IActionResult GetScopeInitialToken()
  {
    var token = _tokenBuilder.BuildScopeInitialAccessToken();
    return Ok(new GetScopeInitialAccessToken
    {
      AccessToken = token,
      ExpiresIn = 300
    });
  }

  [HttpPost]
  [Route("register")]
  [Authorize(Policy = AuthorizationConstants.ScopeRegistration)]
  [ProducesResponseType(typeof(PostScopeResponse), StatusCodes.Status201Created)]
  public async Task<IActionResult> PostAsync([FromBody] PostScopeRequest request,
    CancellationToken cancellationToken = default)
  {
    var command = new CreateScopeCommand
    {
      ScopeName = request.ScopeName
    };
    var response = await _mediator.Send(command, cancellationToken: cancellationToken);
    
    if (response.IsError())
      return this.BadOAuthResult(response.ErrorCode!, response.ErrorDescription!);

    var uri = $"{Request.Scheme}://{Request.Host}/connect/scope/configuration";
    return Created(new Uri(uri), new PostScopeResponse
    {
      Id = response.Id,
      ScopeName = response.ScopeName,
      ScopeRegistrationAccessToken = response.ScopeRegistrationAccessToken
    });
  }
}