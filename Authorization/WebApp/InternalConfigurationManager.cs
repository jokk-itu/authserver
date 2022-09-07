using Contracts.GetDiscovery;
using Contracts.GetJwksDocument;
using Infrastructure;
using Infrastructure.Repositories;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using System.Text.Json;

namespace WebApp;

public class InternalConfigurationManager : IConfigurationManager<OpenIdConnectConfiguration>
{
  private readonly JwkManager _jwkManager;
  private readonly ScopeManager _scopeManager;
  private readonly IdentityConfiguration _identityConfiguration;
  private OpenIdConnectConfiguration? _openIdConnectConfiguration;

  public InternalConfigurationManager(JwkManager jwkManager, IdentityConfiguration identityConfiguration, IServiceProvider serviceProvider)
  {
    var scope = serviceProvider.CreateScope();
    _jwkManager = jwkManager;
    _scopeManager = scope.ServiceProvider.GetRequiredService<ScopeManager>();
    _identityConfiguration = identityConfiguration;
  }

  public async Task<OpenIdConnectConfiguration> GetConfigurationAsync(CancellationToken cancel)
  {
    if(_openIdConnectConfiguration is null)
    {
      await RefreshAsync();
      return _openIdConnectConfiguration ?? throw new Exception("Configuration is not available");
    }
    return _openIdConnectConfiguration;
  }

  public void RequestRefresh()
  {
    Task.Run(RefreshAsync);
  }

  private async Task RefreshAsync()
  {
    var discoveryDocument = new GetDiscoveryDocumentResponse
    {
      Issuer = _identityConfiguration.InternalIssuer,
      AuthorizationEndpoint = $"{_identityConfiguration.ExternalIssuer}/connect/v1/authorize",
      TokenEndpoint = $"{_identityConfiguration.InternalIssuer}/connect/v1/token",
      UserInfoEndpoint = $"{_identityConfiguration.InternalIssuer}/connect/v1/account/userinfo",
      JwksUri = $"{_identityConfiguration.InternalIssuer}/.well-known/jwks",
      Scopes = (await _scopeManager.ReadScopesAsync()).Select(scope => scope.Name)
    };
    _openIdConnectConfiguration = OpenIdConnectConfiguration.Create(JsonSerializer.Serialize(discoveryDocument));

    var jwkDocument = new GetJwksDocumentResponse();
    foreach (var jwk in _jwkManager.Jwks)
    {
      jwkDocument.Keys.Add(new JwkDto
      {
        KeyId = jwk.KeyId,
        Modulus = Base64UrlEncoder.Encode(jwk.Modulus),
        Exponent = Base64UrlEncoder.Encode(jwk.Exponent)
      });
    }
    _openIdConnectConfiguration.JsonWebKeySet = JsonWebKeySet.Create(JsonSerializer.Serialize(jwkDocument));
    foreach (var signingKey in _openIdConnectConfiguration.JsonWebKeySet.GetSigningKeys())
      _openIdConnectConfiguration.SigningKeys.Add(signingKey);
  }
}
