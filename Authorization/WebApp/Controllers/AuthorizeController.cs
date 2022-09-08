using Infrastructure.Repositories;
using Infrastructure.TokenFactories;
using Contracts.AuthorizeCode;
using Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebApp.Constants;
using Domain.Constants;
using System.Text.RegularExpressions;
using WebApp.Extensions;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.OpenApi.Validations.Rules;

namespace WebApp.Controllers;

[Route("connect/v1/[controller]")]
public class AuthorizeController : Controller
{
  private readonly UserManager<User> _userManager;
  private readonly ClientManager _clientManager;
  private readonly CodeFactory _authorizationCodeTokenFactory;
  private readonly CodeManager _codeManager;
  private readonly NonceManager _nonceManager;
  private readonly ILogger<AuthorizeController> _logger;

  public AuthorizeController(
    UserManager<User> userManager,
    ClientManager clientManager,
    CodeFactory authorizationCodeTokenFactory,
    CodeManager codeManager, 
    NonceManager nonceManager,
    ILogger<AuthorizeController> logger)
  {
    _userManager = userManager;
    _clientManager = clientManager;
    _authorizationCodeTokenFactory = authorizationCodeTokenFactory;
    _codeManager = codeManager;
    _nonceManager = nonceManager;
    _logger = logger;
  }

  [HttpGet]
  public IActionResult Index()
  {
    return View();
  }

  [ValidateAntiForgeryToken]
  [HttpPost]
  [Consumes("application/x-www-form-urlencoded")]
  [ProducesResponseType(StatusCodes.Status302Found)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  public async Task<IActionResult> PostAuthorizeAsync(
    [FromForm] PostAuthorizeCodeRequest request,
    [FromQuery(Name = ParameterNames.ResponseType)] string responseType,
    [FromQuery(Name = ParameterNames.ClientId)] string clientId,
    [FromQuery(Name = ParameterNames.RedirectUri)] string redirectUri,
    [FromQuery(Name = ParameterNames.Scope)] string scope,
    [FromQuery(Name = ParameterNames.State)] string state,
    [FromQuery(Name = ParameterNames.CodeChallenge)] string codeChallenge,
    [FromQuery(Name = ParameterNames.CodeChallengeMethod)] string codeChallengeMethod,
    [FromQuery(Name = ParameterNames.Nonce)] string nonce,
    CancellationToken cancellationToken = default)
  {
    if (string.IsNullOrWhiteSpace(clientId))
      return this.BadOAuthResult(ErrorCode.InvalidRequest);

    if (string.IsNullOrWhiteSpace(redirectUri))
      return this.BadOAuthResult(ErrorCode.InvalidRequest);

    var client = await _clientManager.ReadClientAsync(clientId, cancellationToken: cancellationToken);
    if (client is null)
      return this.BadOAuthResult(ErrorCode.InvalidRequest);

    if (!_clientManager.IsAuthorizedRedirectUris(client, new string[] { redirectUri }))
      return this.BadOAuthResult(ErrorCode.InvalidRequest);

    if (string.IsNullOrWhiteSpace(state))
    {
      var stateQuery = new QueryBuilder 
      { 
        { "error", ErrorCode.InvalidRequest } 
      }.ToQueryString();
      return Redirect($"{redirectUri}{stateQuery}");
    }

    var scopes = scope.Split(' ');
    if (!scopes.Contains(ScopeConstants.OpenId))
      return this.RedirectOAuthResult(redirectUri, state, ErrorCode.InvalidScope, "scope is invalid");

    if (!_clientManager.IsAuthorizedScopes(client, scopes))
      return this.RedirectOAuthResult(redirectUri, state, ErrorCode.UnauthorizedClient, "client is not authorized for scope");

    if (string.IsNullOrWhiteSpace(codeChallenge) || !Regex.IsMatch(codeChallenge, "^[0-9a-zA-Z-_~.]{43,128}$"))
      return this.RedirectOAuthResult(redirectUri, state, ErrorCode.InvalidRequest, "code_challenge is invalid");

    if (string.IsNullOrWhiteSpace(codeChallengeMethod) || codeChallengeMethod != CodeChallengeMethodConstants.S256)
      return this.RedirectOAuthResult(redirectUri, state, ErrorCode.InvalidRequest, "code_challenge_method is invalid");

    if (string.IsNullOrWhiteSpace(nonce))
      return this.RedirectOAuthResult(redirectUri, state, ErrorCode.InvalidRequest, "nonce is invalid");

    if (responseType != ResponseTypeConstants.Code)
      return this.RedirectOAuthResult(redirectUri, state, ErrorCode.UnsupportedResponseType, "response_type is invalid");

    var user = await _userManager.FindByNameAsync(request.Username);
    if (user is null)
      return this.RedirectOAuthResult(redirectUri, state, ErrorCode.AccessDenied, "user not found");

    var isPasswordValid = await _userManager.CheckPasswordAsync(user, request.Password);
    if (!isPasswordValid)
      return this.RedirectOAuthResult(redirectUri, state, ErrorCode.AccessDenied, "user not found");

    var storedNonce = await _nonceManager.ReadNonceAsync(nonce, cancellationToken: cancellationToken);
    if (storedNonce is not null)
    {
      _logger.LogWarning("Nonce {Nonce} replay attack detected", nonce);
      return this.RedirectOAuthResult(redirectUri, state, ErrorCode.AccessDenied, "nonce replay attack detected");
    }

    var code = await _authorizationCodeTokenFactory.GenerateCodeAsync(
        redirectUri,
        scopes,
        clientId,
        codeChallenge,
        user.Id,
        nonce);

    var isCodeCreated = await _codeManager.CreateAuthorizationCodeAsync(client, code, cancellationToken: cancellationToken);
    if (!isCodeCreated)
      return this.RedirectOAuthResult(redirectUri, state, ErrorCode.ServerError);

    var isNonceCreated = await _nonceManager.CreateNonceAsync(nonce, cancellationToken: cancellationToken);
    if (!isNonceCreated)
      return this.RedirectOAuthResult(redirectUri, state, ErrorCode.ServerError);

    var codeQuery = new QueryBuilder 
    {
      { "code", code },
      { "state", state }
    }.ToQueryString();

    return Redirect($"{redirectUri}{codeQuery}");
  }
}