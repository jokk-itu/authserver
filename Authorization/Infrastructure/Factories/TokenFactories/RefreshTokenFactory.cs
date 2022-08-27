using Infrastructure.Repositories;
using Infrastructure.Factories.TokenFactories.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Options;
using Domain.Constants;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Infrastructure.Factories.TokenFactories;

public class RefreshTokenFactory : TokenFactory
{
  private readonly ResourceManager _resourceManager;

  public RefreshTokenFactory(
    IdentityConfiguration identityConfiguration,
    IOptionsMonitor<JwtBearerOptions> jwtBearerOptions,
    ResourceManager resourceManager,
    JwkManager jwkManager,
    ILogger<RefreshTokenFactory> logger)
    : base(logger, identityConfiguration, jwtBearerOptions.Get(OpenIdConnectDefaults.AuthenticationScheme), jwkManager)
  {
    _resourceManager = resourceManager;
  }

  public async Task<string> GenerateTokenAsync(string clientId, ICollection<string> scopes, string userId, CancellationToken cancellationToken = default)
  {
    var expires = DateTime.Now + TimeSpan.FromSeconds(_identityConfiguration.RefreshTokenExpiration);
    var resources = await _resourceManager.ReadResourcesAsync(scopes, cancellationToken);
    var audiences = resources.Select(x => x.Name).ToArray();
    var claims = new Dictionary<string, object>
    {
      { JwtRegisteredClaimNames.Sub, userId },
      { JwtRegisteredClaimNames.Aud, audiences },
      { ClaimNameConstants.Scope, string.Join(' ', scopes) },
      { ClaimNameConstants.ClientId, clientId }
    };
    return GetSignedToken(claims, expires);
  }
}