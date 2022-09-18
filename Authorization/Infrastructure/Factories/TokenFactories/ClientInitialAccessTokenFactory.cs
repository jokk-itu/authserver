using Domain.Constants;
using Infrastructure.Factories.TokenFactories.Abstractions;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.JsonWebTokens;

namespace Infrastructure.Factories.TokenFactories;
public class ClientInitialAccessTokenFactory : TokenFactory
{
  public ClientInitialAccessTokenFactory(
    ILogger<TokenFactory> logger, 
    IdentityConfiguration identityConfiguration, 
    JwtBearerOptions jwtBearerOptions, 
    JwkManager jwkManager) : base(logger, identityConfiguration, jwtBearerOptions, jwkManager)
  {
  }

  public string GenerateToken()
  {
    var expires = DateTime.Now.AddSeconds(300);
    var claims = new Dictionary<string, object> 
    {
      { JwtRegisteredClaimNames.Aud, AudienceConstants.IdentityProvider },
      { ClaimNameConstants.Scope, string.Join(' ', ScopeConstants.ClientRegistration) },
    };
    return GetSignedToken(claims, expires);
  }
}
