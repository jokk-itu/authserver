using System.Security.Policy;
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
  public const string RedirectUris = "redirect_uris";
  public const string ResponseTypes = "response_types";
  public const string GrantTypes = "grant_types";
  public const string ApplicationType = "application_type";
  public const string Contacts = "contacts";
  public const string ClientName = "client_name";
  public const string PolicyUri = "policy_uri";
  public const string TosUri = "tos_uri";
  public const string SubjectType = "subject_type";
  public const string TokenEndpointAuthMethod = "token_endpoint_auth_method";
  public const string ClientSecretExpiresAt = "client_secret_expires_at";
  public const string RegistrationAccessToken = "registration_access_token";
  public const string RegistrationClientUri = "registration_client_uri";
}