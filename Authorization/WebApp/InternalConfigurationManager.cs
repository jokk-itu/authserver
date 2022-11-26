using Contracts.GetJwksDocument;
using Infrastructure.Repositories;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using System.Text.Json;
using Application;
using Domain;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using WebApp.Contracts.GetDiscoveryDocument;

namespace WebApp;

public class InternalConfigurationManager : IConfigurationManager<OpenIdConnectConfiguration>
{
  private readonly JwkManager _jwkManager;
  private readonly IdentityConfiguration _identityConfiguration;
  private OpenIdConnectConfiguration? _openIdConnectConfiguration;
  private readonly IdentityContext _identityContext;

  public InternalConfigurationManager(JwkManager jwkManager, IdentityConfiguration identityConfiguration, IServiceProvider serviceProvider)
  {
    var scope = serviceProvider.CreateScope();
    _jwkManager = jwkManager;
    _identityContext = scope.ServiceProvider.GetRequiredService<IdentityContext>();
    _identityConfiguration = identityConfiguration;
  }

  public async Task<OpenIdConnectConfiguration> GetConfigurationAsync(CancellationToken cancel)
  {
    if (_openIdConnectConfiguration is not null) 
      return _openIdConnectConfiguration;

    await RefreshAsync();
    return _openIdConnectConfiguration ?? throw new Exception("Configuration is not available");
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
      AuthorizationEndpoint = $"{_identityConfiguration.ExternalIssuer}/connect/authorize",
      TokenEndpoint = $"{_identityConfiguration.InternalIssuer}/connect/token",
      UserInfoEndpoint = $"{_identityConfiguration.InternalIssuer}/connect/userinfo",
      JwksUri = $"{_identityConfiguration.InternalIssuer}/.well-known/jwks",
      Scopes = await _identityContext.Set<Scope>().Select(x => x.Name).ToListAsync()
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