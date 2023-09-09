using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using System.Text.Json;
using Infrastructure.Builders.Abstractions;
using Mapster;
using WebApp.Contracts.GetDiscoveryDocument;
using WebApp.Contracts.GetJwksDocument;

namespace WebApp;

public class InternalConfigurationManager : IConfigurationManager<OpenIdConnectConfiguration>
{
  private readonly IDiscoveryBuilder _builder;
  private OpenIdConnectConfiguration? _openIdConnectConfiguration;

  public InternalConfigurationManager(IServiceProvider serviceProvider)
  {
    var scope = serviceProvider.CreateScope();
    _builder = scope.ServiceProvider.GetRequiredService<IDiscoveryBuilder>();
  }

  public async Task<OpenIdConnectConfiguration> GetConfigurationAsync(CancellationToken cancel)
  {
    if (_openIdConnectConfiguration is not null)
    {
      return _openIdConnectConfiguration;
    }

    await RefreshAsync();
    return _openIdConnectConfiguration ?? throw new Exception("Configuration is not available");
  }

  public void RequestRefresh()
  {
    Task.Run(RefreshAsync);
  }

  private async Task RefreshAsync()
  {
    var discoveryDocument = _builder.BuildDiscoveryDocument().Adapt<GetDiscoveryDocumentResponse>();
    var jwkDocument = (await _builder.BuildJwkDocument()).Adapt<GetJwksDocumentResponse>();

    _openIdConnectConfiguration = OpenIdConnectConfiguration.Create(JsonSerializer.Serialize(discoveryDocument));
    _openIdConnectConfiguration.JsonWebKeySet = JsonWebKeySet.Create(JsonSerializer.Serialize(jwkDocument));

    foreach (var signingKey in _openIdConnectConfiguration.JsonWebKeySet.GetSigningKeys())
    {
      _openIdConnectConfiguration.SigningKeys.Add(signingKey);
    }
  }
}