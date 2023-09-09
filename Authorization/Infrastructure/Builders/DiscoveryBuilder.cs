using Application;
using Domain.Constants;
using Infrastructure.Builders.Abstractions;
using Infrastructure.Repositories;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Builders;

#nullable disable
public class DiscoveryBuilder : IDiscoveryBuilder
{
  private const string Algorithm = "RSA";
  private const string Use = "sig";
  private readonly string[] Scopes =
  {
    ScopeConstants.OpenId,
    ScopeConstants.ClientConfiguration,
    ScopeConstants.Email,
    ScopeConstants.OfflineAccess,
    ScopeConstants.Phone,
    ScopeConstants.Profile,
    ScopeConstants.UserInfo
  };
  private readonly string[] ClaimsSupported =
  {
    ClaimNameConstants.Address,
    ClaimNameConstants.Birthdate,
    ClaimNameConstants.Email,
    ClaimNameConstants.Phone,
    ClaimNameConstants.Name,
    ClaimNameConstants.GivenName,
    ClaimNameConstants.FamilyName,
    ClaimNameConstants.Locale,
    ClaimNameConstants.Nonce,
    ClaimNameConstants.AuthTime,
    ClaimNameConstants.Sub,
    ClaimNameConstants.Sid,
    ClaimNameConstants.GrantId,
    ClaimNameConstants.Aud,
    ClaimNameConstants.Scope
  };

  private readonly IdentityConfiguration _identityConfiguration;
  private readonly JwkManager _jwkManager;

  public DiscoveryBuilder(
    IdentityConfiguration identityConfiguration,
    JwkManager jwkManager)
  {
    _identityConfiguration = identityConfiguration;
    _jwkManager = jwkManager;
  }

  public DiscoveryDocument BuildDiscoveryDocument()
  {
    var issuer = _identityConfiguration.Issuer;
    var serviceDocumentation = _identityConfiguration.ServiceDocumentation;
    
    return new DiscoveryDocument
    {
      Issuer = issuer,
      ServiceDocumentation = serviceDocumentation,
      AuthorizationEndpoint = $"{issuer}/connect/authorize",
      TokenEndpoint = $"{issuer}/connect/token",
      UserInfoEndpoint = $"{issuer}/connect/userinfo",
      JwksUri = $"{issuer}/.well-known/jwks",
      EndSessionEndpoint = $"{issuer}/connect/end-session",
      IntrospectionEndpoint = $"{issuer}/connect/introspection",
      RevocationEndpoint = $"{issuer}/connect/revoke",
      RegistrationEndpoint = $"{issuer}/connect/register",
      Scopes = Scopes,
      GrantTypes = GrantTypeConstants.GrantTypes,
      ResponseTypes = ResponseTypeConstants.ResponseTypes,
      TokenEndpointAuthMethods = TokenEndpointAuthMethodConstants.AuthMethods,
      TokenEndpointAuthSigningAlgValues = TokenEndpointSigningAlgConstants.SigningAlgorithms,
      IntrospectionEndpointAuthMethodsSupported = IntrospectionEndpointAuthMethodConstants.AuthMethods,
      RevocationEndpointAuthMethodsSupported = RevocationEndpointAuthMethodConstants.AuthMethods,
      CodeChallengeMethods = CodeChallengeMethodConstants.CodeChallengeMethods,
      ResponseModes = ResponseModeConstants.ResponseModes,
      SubjectTypes = SubjectTypeConstants.SubjectTypes,
      IdTokenSigningAlgValues = IdTokenSigningAlgConstants.IdTokenSigningAlgorithms,
      ClaimsSupported = ClaimsSupported,
      AuthorizationResponseIssParameterSupported = true,
      BackChannelLogoutSupported = true,
      RequestUriParameterSupported = false
    };
  }

  public Task<JwksDocument> BuildJwkDocument()
  {
    var keys = _jwkManager.Jwks
      .Select(jwk => new Jwk
      {
        KeyType = Algorithm,
        Use = Use,
        Alg = Algorithm,
        KeyId = jwk.Id.ToString(),
        Modulus = Base64UrlEncoder.Encode(jwk.Modulus),
        Exponent = Base64UrlEncoder.Encode(jwk.Exponent)
      })
      .ToList();

    return Task.FromResult(new JwksDocument
    {
      Keys = keys
    });
  }
}

public class DiscoveryDocument
{
  public string Issuer { get; init; }
  public string ServiceDocumentation { get; init; }
  public string AuthorizationEndpoint { get; init; }
  public string TokenEndpoint { get; init; }
  public string UserInfoEndpoint { get; init; }
  public string JwksUri { get; init; }
  public string EndSessionEndpoint { get; init; }
  public string IntrospectionEndpoint { get; init; }
  public string RevocationEndpoint { get; init; }
  public string RegistrationEndpoint { get; init; }
  public IEnumerable<string> Scopes { get; init; }
  public IEnumerable<string> ResponseTypes { get; init; }
  public IEnumerable<string> GrantTypes { get; init; }
  public IEnumerable<string> TokenEndpointAuthMethods { get; init; }
  public IEnumerable<string> TokenEndpointAuthSigningAlgValues { get; init; }
  public IEnumerable<string> IntrospectionEndpointAuthMethodsSupported { get; init; }
  public IEnumerable<string> RevocationEndpointAuthMethodsSupported { get; init; }
  public IEnumerable<string> CodeChallengeMethods { get; init; }
  public IEnumerable<string> SubjectTypes { get; init; }
  public IEnumerable<string> IdTokenSigningAlgValues { get; init; }
  public IEnumerable<string> ResponseModes { get; init; }
  public IEnumerable<string> ClaimsSupported { get; init; }
  public bool AuthorizationResponseIssParameterSupported { get; init; }
  public bool BackChannelLogoutSupported { get; init; }
  public bool RequestUriParameterSupported { get; init; }
}

public class JwksDocument
{
  public IEnumerable<Jwk> Keys { get; set; } = new List<Jwk>();
}

public class Jwk
{
  public string KeyType { get; init; }
  public string Use { get; init; }
  public string KeyId { get; init; }
  public string Alg { get; init; }
  public string Modulus { get; init; }
  public string Exponent { get; init; }
}