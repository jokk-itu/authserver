using System.Text.RegularExpressions;
using Domain.Constants;
using Infrastructure.Helpers;
using Microsoft.AspNetCore.Http.Extensions;
using WebApp.Constants;

namespace Specs.Helpers.EndpointBuilders;
public class AuthorizeEndpointBuilder
{
  private readonly QueryBuilder _queryBuilder = new();

  private string _codeChallenge = string.Empty;
  private string _responseType = ResponseTypeConstants.Code;
  private string _clientId = string.Empty;
  private string _redirectUri = string.Empty;
  private string _scope = string.Empty;
  private string _maxAge = string.Empty;
  private string _prompt = string.Empty;
  private string _idTokenHint = string.Empty;

  private string _userName = string.Empty;
  private string _password = string.Empty;

  private AuthorizeEndpointBuilder()
  {
    _queryBuilder.Add(ParameterNames.CodeChallengeMethod, CodeChallengeMethodConstants.S256);
    _queryBuilder.Add(ParameterNames.State, CryptographyHelper.GetRandomString(16));
    _queryBuilder.Add(ParameterNames.Nonce, CryptographyHelper.GetRandomString(16));
  }

  public static AuthorizeEndpointBuilder Instance()
  {
    return new AuthorizeEndpointBuilder();
  }

  public AuthorizeEndpointBuilder AddUser(string userName, string password)
  {
    _userName = userName;
    _password = password;
    return this;
  }

  public AuthorizeEndpointBuilder AddCodeChallenge(string codeChallenge)
  {
    _codeChallenge = codeChallenge;
    return this;
  }

  public AuthorizeEndpointBuilder AddClientId(string clientId)
  {
    _clientId = clientId;
    return this;
  }

  public AuthorizeEndpointBuilder AddRedirectUri(string redirectUri)
  {
    _redirectUri = redirectUri;
    return this;
  }

  public AuthorizeEndpointBuilder AddScope(string scope)
  {
    _scope = scope;
    return this;
  }

  public AuthorizeEndpointBuilder AddMaxAge(string maxAge)
  {
    _maxAge = maxAge;
    return this;
  }

  public AuthorizeEndpointBuilder AddPrompt(string prompt)
  {
    _prompt = prompt;
    return this;
  }

  public AuthorizeEndpointBuilder AddIdTokenHint(string idTokenHint)
  {
    _idTokenHint = idTokenHint;
    return this;
  }

  private void BuildQuery()
  {
    _queryBuilder.Add(ParameterNames.CodeChallenge, _codeChallenge);
    _queryBuilder.Add(ParameterNames.ResponseType, _responseType);
    _queryBuilder.Add(ParameterNames.ClientId, _clientId);
    _queryBuilder.Add(ParameterNames.RedirectUri, _redirectUri);
    _queryBuilder.Add(ParameterNames.Scope, _scope);
    _queryBuilder.Add(ParameterNames.MaxAge, _maxAge);
    _queryBuilder.Add(ParameterNames.Prompt, _prompt);
    _queryBuilder.Add(ParameterNames.IdTokenHint, _idTokenHint);
  }

  public async Task<string> BuildLogin(HttpClient httpClient, CancellationToken cancellationToken = default)
  {
    var antiForgeryTokenHelper = new AntiForgeryTokenHelper(httpClient);
    BuildQuery();
    var authorizeResponse = await httpClient.GetAsync($"/connect/authorize?{_queryBuilder}", cancellationToken: cancellationToken);
    var locationHeader = authorizeResponse.Headers.Location;
    var loginAntiForgery = await antiForgeryTokenHelper.GetAntiForgeryToken(locationHeader!.ToString());
    var loginResponse = await LoginEndpointHelper.Login(httpClient, _queryBuilder.ToQueryString(), _userName, _password,
      loginAntiForgery);
    var html = await loginResponse.Content.ReadAsStringAsync(cancellationToken);
    var authorizationCodeInput = Regex.Match(html, @"\<input name=""code"" type=""hidden"" value=""([^""]+)"" \/\>");
    return authorizationCodeInput.Groups[1].Captures[0].Value;
  }

  public async Task<string> BuildNone(HttpClient httpClient, CancellationToken cancellationToken = default)
  {
    BuildQuery();
    var authorizeResponse = await httpClient.GetAsync($"/connect/authorize?{_queryBuilder}", cancellationToken: cancellationToken);
    var html = await authorizeResponse.Content.ReadAsStringAsync(cancellationToken);
    var authorizationCodeInput = Regex.Match(html, @"\<input name=""code"" type=""hidden"" value=""([^""]+)"" \/\>");
    return authorizationCodeInput.Groups[1].Captures[0].Value;
  }

  public async Task<string> BuildLoginAndConsent(HttpClient httpClient, CancellationToken cancellationToken = default)
  {
    var antiForgeryTokenHelper = new AntiForgeryTokenHelper(httpClient);
    BuildQuery();
    var authorizeResponse = await httpClient.GetAsync($"/connect/authorize?{_queryBuilder}", cancellationToken: cancellationToken);
    var locationHeader = authorizeResponse.Headers.Location;
    var loginAntiForgery = await antiForgeryTokenHelper.GetAntiForgeryToken(locationHeader!.ToString());
    var loginResponse = await LoginEndpointHelper.Login(httpClient, _queryBuilder.ToQueryString(), _userName, _password, loginAntiForgery);
    var loginCookie = loginResponse.Headers.GetValues("Set-Cookie").Single();
    var consentAntiForgery = await antiForgeryTokenHelper.GetAntiForgeryToken($"connect/consent/create{_queryBuilder}", loginCookie);
    var consentResponse = await ConsentEndpointHelper.GetConsent(httpClient, _queryBuilder.ToQueryString(), consentAntiForgery, loginCookie);
    var html = await consentResponse.Content.ReadAsStringAsync(cancellationToken);
    var authorizationCodeInput = Regex.Match(html, @"\<input name=""code"" type=""hidden"" value=""([^""]+)"" \/\>");
    return authorizationCodeInput.Groups[1].Captures[0].Value;
  }
}