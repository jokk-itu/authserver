using Domain.Constants;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Infrastructure.Requests.GeUserInfo;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using WebApp.Extensions;

namespace WebApp.Controllers;

[ApiController]
[Route("connect/[controller]")]
public class UserInfoController : Controller
{
  private readonly IMediator _mediator;

  public UserInfoController(
    IMediator mediator)
  {
    _mediator = mediator;
  }

  [HttpGet]
  [Authorize]
  [ProducesResponseType(StatusCodes.Status200OK)]
  public async Task<IActionResult> GetAsync(CancellationToken cancellationToken = default)
  {
    var accessToken = await HttpContext.GetTokenAsync(JwtBearerDefaults.AuthenticationScheme, TokenTypeConstants.AccessToken);
    var query = new GetUserInfoQuery
    {
      AccessToken = accessToken
    };
    var response = await _mediator.Send(query, cancellationToken: cancellationToken);
    if (response.IsError())
      return this.BadOAuthResult(response.ErrorCode, response.ErrorDescription);

    return Json(response.UserInfo);
  }
}
