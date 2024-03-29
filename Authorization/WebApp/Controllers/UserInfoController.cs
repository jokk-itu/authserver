﻿using Application;
using Domain.Constants;
using Infrastructure.Requests.GetUserInfo;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using WebApp.Attributes;
using WebApp.Constants;
using WebApp.Controllers.Abstracts;

namespace WebApp.Controllers;

[ApiController]
[Route("connect/[controller]")]
public class UserInfoController : OAuthControllerBase
{
  private readonly IMediator _mediator;

  public UserInfoController(
    IMediator mediator,
    IdentityConfiguration identityConfiguration) : base(identityConfiguration)
  {
    _mediator = mediator;
  }

  [HttpGet]
  [HttpPost]
  [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = AuthorizationConstants.UserInfo)]
  [SecurityHeader]
  [ProducesResponseType(StatusCodes.Status200OK)]
  public async Task<IActionResult> Get(CancellationToken cancellationToken = default)
  {
    var accessToken = await HttpContext.GetTokenAsync(JwtBearerDefaults.AuthenticationScheme, TokenTypeConstants.AccessToken);
    var query = new GetUserInfoQuery
    {
      AccessToken = accessToken
    };
    var response = await _mediator.Send(query, cancellationToken: cancellationToken);
    if (response.IsError())
    {
      return BadOAuthResult(response.ErrorCode, response.ErrorDescription);
    }

    return Json(response.UserInfo);
  }
}