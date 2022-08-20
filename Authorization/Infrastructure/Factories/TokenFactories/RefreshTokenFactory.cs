using Infrastructure;
using Infrastructure.Repositories;
using Infrastructure.Factories.TokenFactories.Abstractions;
using Infrastructure.Repositories;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Domain.Constants;

namespace Infrastructure.Factories.TokenFactories;

public class RefreshTokenFactory : TokenFactory
{
  private readonly ResourceManager _resourceManager;

  public RefreshTokenFactory(
    IdentityConfiguration identityConfiguration,
    TokenValidationParameters tokenValidationParameters,
    ResourceManager resourceManager,
    JwkManager jwkManager,
    ILogger<RefreshTokenFactory> logger)
    : base(logger, identityConfiguration, tokenValidationParameters, jwkManager)
  {
    _resourceManager = resourceManager;
  }

  public async Task<string> GenerateTokenAsync(string clientId, ICollection<string> scopes, string userId)
  {
    var expires = DateTime.Now + TimeSpan.FromSeconds(_identityConfiguration.RefreshTokenExpiration);
    var resources = await _resourceManager.ReadResourcesAsync(scopes);
    var audience = string.Join(' ', resources.Select(x => x.Id));
    var claims = new[]
    {
      new Claim(JwtRegisteredClaimNames.Sub, userId),
      new Claim(ClaimNameConstants.Scope, string.Join(' ', scopes)),
      new Claim(ClaimNameConstants.ClientId, clientId)
    };

    return GetSignedToken(claims, audience, expires);
  }
}