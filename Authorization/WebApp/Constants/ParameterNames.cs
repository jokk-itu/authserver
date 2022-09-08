using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace WebApp.Constants;

public static class ParameterNames
{
  public const string ResponseType = OpenIdConnectParameterNames.ResponseType;
  public const string ClientId = OpenIdConnectParameterNames.ClientId;
  public const string ClientSecret = OpenIdConnectParameterNames.ClientSecret;
  public const string RedirectUri = OpenIdConnectParameterNames.RedirectUri;
  public const string Scope = OpenIdConnectParameterNames.Scope;
  public const string State = OpenIdConnectParameterNames.State;
  public const string CodeChallenge = "code_challenge";
  public const string CodeChallengeMethod = "code_challenge_method";
  public const string CodeVerifier = "code_verifier";
  public const string Nonce = OpenIdConnectParameterNames.Nonce;
  public const string Code = OpenIdConnectParameterNames.Code;
  public const string RefreshToken = OpenIdConnectParameterNames.RefreshToken;
  public const string AccessToken = OpenIdConnectParameterNames.AccessToken;
  public const string IdToken = OpenIdConnectParameterNames.IdToken;
  public const string TokenType = OpenIdConnectParameterNames.TokenType;
  public const string ExpiresIn = OpenIdConnectParameterNames.ExpiresIn;
  public const string GrantType = OpenIdConnectParameterNames.GrantType;
}
