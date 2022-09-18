using Infrastructure.Repositories;
using Infrastructure.Factories.TokenFactories.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Domain.Constants;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Infrastructure.Factories.TokenFactories;

public class AccessTokenFactory : TokenFactory
{
  private readonly ResourceManager _resourceManager;

  public AccessTokenFactory(
      IdentityConfiguration identityConfiguration,
      IOptionsSnapshot<JwtBearerOptions> jwtBearerOptions,
      ResourceManager resourceManager,
      JwkManager jwkManager,
      ILogger<AccessTokenFactory> logger)
    : base(logger, identityConfiguration, jwtBearerOptions.Get(OpenIdConnectDefaults.AuthenticationScheme), jwkManager)
  {
    _resourceManager = resourceManager;
  }

  public async Task<string> GenerateTokenAsync(
    string clientId, 
    ICollection<string> scopes, 
    string userId, 
    CancellationToken cancellationToken = default)
  {
    var expires = DateTime.Now.AddSeconds(_identityConfiguration.AccessTokenExpiration);
    var resources = await _resourceManager.ReadResourcesAsync(scopes, cancellationToken);
    var audiences = resources.Select(x => x.Id).ToArray();
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