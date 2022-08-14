using Domain;
using Infrastructure.Repositories;
using Infrastructure.TokenFactories;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;

namespace AuthorizationServer.TokenFactories;

public class IdTokenFactory : TokenFactory
{
  private readonly UserManager<IdentityUserExtended> _userManager;

  public IdTokenFactory(
    IdentityConfiguration identityConfiguration,
    TokenValidationParameters tokenValidationParameters,
    UserManager<IdentityUserExtended> userManager,
    JwkManager jwkManager,
    ILogger<IdTokenFactory> logger)
    : base(logger, identityConfiguration, tokenValidationParameters, jwkManager)
  {
    _userManager = userManager;
  }

  public async Task<string> GenerateTokenAsync(string clientId, IEnumerable<string> scopes,
      string nonce, string userId)
  {
    var expires = DateTime.Now + TimeSpan.FromSeconds(_identityConfiguration.IdTokenExpiration);
    var audience = clientId;
    var claims = new List<Claim>
    {
      new(JwtRegisteredClaimNames.Sub, userId),
      new("scope", string.Join(' ', scopes)),
      new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString())
    };

    if (!string.IsNullOrWhiteSpace(nonce))
      claims.Add(new(JwtRegisteredClaimNames.Nonce, nonce));

    var user = await _userManager.FindByIdAsync(userId);
    var userClaims = await _userManager.GetClaimsAsync(user);
    var userRoles = await _userManager.GetRolesAsync(user);
    claims.AddRange(userClaims.Select(claim => new Claim(claim.Type, claim.Value)));
    claims.Add(new Claim("roles", JsonSerializer.Serialize(userRoles)));

    return GetSignedToken(claims, audience, expires);
  }
}