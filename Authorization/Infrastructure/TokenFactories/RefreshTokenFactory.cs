using AuthorizationServer.Repositories;
using Infrastructure.Repositories;
using Infrastructure.TokenFactories;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace AuthorizationServer.TokenFactories;

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
    var resources = await _resourceManager.FindResourcesByScopes(scopes);
    var audience = string.Join(' ', resources.Select(x => x.Id));
    var claims = new[]
    {
      new Claim(JwtRegisteredClaimNames.Sub, userId),
      new Claim("scope", string.Join(' ', scopes)),
      new Claim("client_id", clientId)
    };

    return GetSignedToken(claims, audience, expires);
  }
}