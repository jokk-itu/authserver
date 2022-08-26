using Infrastructure.Repositories;
using Infrastructure.Factories.TokenFactories.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Domain.Constants;

namespace Infrastructure.Factories.TokenFactories;

public class AccessTokenFactory : TokenFactory
{
  private readonly ResourceManager _resourceManager;

  public AccessTokenFactory(
      IdentityConfiguration identityConfiguration,
      TokenValidationParameters tokenValidationParameters,
      ResourceManager resourceManager,
      JwkManager jwkManager,
      ILogger<AccessTokenFactory> logger)
    : base(logger, identityConfiguration, tokenValidationParameters, jwkManager)
  {
    _resourceManager = resourceManager;
  }

  public async Task<string> GenerateTokenAsync(string clientId, ICollection<string> scopes, string userId, CancellationToken cancellationToken = default)
  {
    var expires = DateTime.Now + TimeSpan.FromSeconds(_identityConfiguration.AccessTokenExpiration);
    var resources = await _resourceManager.ReadResourcesAsync(scopes, cancellationToken);
    var audience = string.Join(' ', resources.Select(x => x.Name));
    var claims = new[]
    {
      new Claim(JwtRegisteredClaimNames.Sub, userId),
      new Claim(ClaimNameConstants.Scope, string.Join(' ', scopes)),
      new Claim(ClaimNameConstants.ClientId, clientId)
    };
    return GetSignedToken(claims, audience, expires);
  }
}