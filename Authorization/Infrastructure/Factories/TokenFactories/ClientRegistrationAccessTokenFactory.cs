using Domain.Constants;
using Infrastructure.Factories.TokenFactories.Abstractions;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;

namespace Infrastructure.Factories.TokenFactories;
public class ClientRegistrationAccessTokenFactory : TokenFactory
{
  public ClientRegistrationAccessTokenFactory(
    ILogger<ClientRegistrationAccessTokenFactory> logger, 
    IdentityConfiguration identityConfiguration, 
    IOptions<JwtBearerOptions> jwtBearerOptions,
    JwkManager jwkManager) : base(logger, identityConfiguration, jwtBearerOptions.Value, jwkManager)
  {
  }

  public string GenerateToken(string clientId)
  {
    var expires = DateTime.MaxValue;
    var claims = new Dictionary<string, object> 
    {
      { JwtRegisteredClaimNames.Aud, AudienceConstants.IdentityProvider },
      { ClaimNameConstants.Scope, string.Join(' ', ScopeConstants.ClientConfiguration) },
      { ClaimNameConstants.ClientId, clientId }
    };
    return GetSignedToken(claims, expires);
  }
}
