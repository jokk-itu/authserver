using Infrastructure.Repositories;
using Infrastructure.TokenFactories;
using Contracts.AuthorizeCode;
using Domain;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebApp.Constants;
using Domain.Constants;
using System.Text.RegularExpressions;
using WebApp.Extensions;

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
    PostAuthorizeCodeRequest request,
    [FromQuery(Name = "response_type")] string responseType,
    [FromQuery(Name = "client_id")] string clientId,
    [FromQuery(Name = "redirect_uri")] string redirectUri,
    [FromQuery(Name = "scope")] string scope,
    [FromQuery(Name = "state")] string state,
    [FromQuery(Name = "code_challenge")] string codeChallenge,
    [FromQuery(Name = "code_challenge_method")] string codeChallengeMethod,
    [FromQuery(Name = "nonce")] string nonce,
    CancellationToken cancellationToken = default)
  {
    if (string.IsNullOrWhiteSpace(clientId))
      return this.BadOAuthRequest(ErrorCode.InvalidRequest);

    if (string.IsNullOrWhiteSpace(redirectUri))
      return this.BadOAuthRequest(ErrorCode.InvalidRequest);

    var client = await _clientManager.ReadClientAsync(clientId, cancellationToken: cancellationToken);
    if (client is null)
      return this.BadOAuthRequest(ErrorCode.InvalidRequest);

    if (!_clientManager.IsAuthorizedRedirectUris(client, new string[] { redirectUri }))
      return this.BadOAuthRequest(ErrorCode.InvalidRequest);

    if (string.IsNullOrWhiteSpace(state))
      return Redirect($"{redirectUri}?error={ErrorCode.InvalidRequest}");

    if (string.IsNullOrWhiteSpace(codeChallenge) || !Regex.IsMatch(codeChallenge, "$[0-9a-zA-Z]{43,128}^"))
      return Redirect($"{redirectUri}?state={state}&error={ErrorCode.InvalidRequest}");

    if (string.IsNullOrWhiteSpace(codeChallengeMethod) || codeChallengeMethod != CodeChallengeMethodConstants.S256)
      return Redirect($"{redirectUri}?state={state}&error={ErrorCode.InvalidRequest}");

    if (string.IsNullOrEmpty(nonce))
      return Redirect($"{redirectUri}?state={state}&error={ErrorCode.InvalidRequest}");

    if (responseType != ResponseTypeConstants.Code)
      return Redirect($"{redirectUri}?state={state}&error={ErrorCode.UnsupportedResponseType}");

    var scopes = scope.Split(' ');
    if (!scopes.Contains(ScopeConstants.OpenId))
      return Redirect($"{redirectUri}?state={state}&error={ErrorCode.InvalidScope}");

    var user = await _userManager.FindByNameAsync(request.Username);
    if (user is null)
      return Redirect($"{redirectUri}?state={state}&error={ErrorCode.AccessDenied}");

    var isPasswordValid = await _userManager.CheckPasswordAsync(user, request.Password);
    if (!isPasswordValid)
      return Redirect($"{redirectUri}?state={state}&error={ErrorCode.AccessDenied}");

    var storedNonce = await _nonceManager.ReadNonceAsync(nonce, cancellationToken: cancellationToken);
    if (storedNonce is not null)
    {
      _logger.LogWarning("Nonce {Nonce} replay detected", nonce);
      return Redirect($"{redirectUri}?state={state}&error={ErrorCode.AccessDenied}");
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
      return Redirect($"{redirectUri}?state={state}&error={ErrorCode.ServerError}");

    var isNonceCreated = await _nonceManager.CreateNonceAsync(nonce, cancellationToken: cancellationToken);
    if (!isNonceCreated)
      return Redirect($"{redirectUri}?state={state}&error={ErrorCode.ServerError}");

    return Redirect($"{redirectUri}?code={code}&state={state}");
  }
}