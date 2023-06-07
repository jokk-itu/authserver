using Application;
using Domain;
using Domain.Constants;
using Infrastructure.Builders.Abstractions;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Builders;

#nullable disable
public class DiscoveryBuilder : IDiscoveryBuilder
{
  private const string Algorithm = "RSA";
  private const string Use = "sig";

  private readonly IdentityConfiguration _identityConfiguration;
  private readonly IdentityContext _identityContext;
  private readonly JwkManager _jwkManager;

  public DiscoveryBuilder(
    IdentityConfiguration identityConfiguration,
    IdentityContext identityContext,
    JwkManager jwkManager)
  {
    _identityConfiguration = identityConfiguration;
    _identityContext = identityContext;
    _jwkManager = jwkManager;
  }

  public async Task<DiscoveryDocument> BuildDiscoveryDocument()
  {
    var issuer = _identityConfiguration.Issuer;
    var scopes = await _identityContext
      .Set<Scope>()
      .Select(x => x.Name)
      .ToListAsync();

    return new DiscoveryDocument
    {
      Issuer = issuer,
      AuthorizationEndpoint = $"{issuer}/connect/authorize",
      TokenEndpoint = $"{issuer}/connect/token",
      UserInfoEndpoint = $"{issuer}/connect/userinfo",
      JwksUri = $"{issuer}/.well-known/jwks.json",
      EndSessionEndpoint = $"{issuer}/connect/end-session",
      IntrospectionEndpoint = $"{issuer}/connect/introspection",
      RevocationEndpoint = $"{issuer}/connect/revoke",
      RegistrationEndpoint = $"{issuer}/connect/register",
      Scopes = scopes,
      GrantTypes = GrantTypeConstants.GrantTypes,
      ResponseTypes = ResponseTypeConstants.ResponseTypes,
      TokenEndpointAuthMethods = TokenEndpointAuthMethodConstants.AuthMethods,
      TokenEndpointAuthSigningAlgValues = TokenEndpointSigningAlgConstants.SigningAlgorithms,
      IntrospectionEndpointAuthMethodsSupported = IntrospectionEndpointAuthMethodConstants.AuthMethods,
      RevocationEndpointAuthMethodsSupported = RevocationEndpointAuthMethodConstants.AuthMethods,
      CodeChallengeMethods = CodeChallengeMethodConstants.CodeChallengeMethods,
      ResponseModes = ResponseModeConstants.ResponseModes,
      SubjectTypes = SubjectTypeConstants.SubjectTypes,
      AuthorizationResponseIssParameterSupported = true,
      BackChannelLogoutSupported = true,
      IdTokenSigningAlgValues = IdTokenSigningAlgConstants.IdTokenSigningAlgorithms
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
  public bool AuthorizationResponseIssParameterSupported { get; init; }
  public bool BackChannelLogoutSupported { get; init; }
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