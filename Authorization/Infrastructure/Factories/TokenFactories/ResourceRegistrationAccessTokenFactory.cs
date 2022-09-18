using Domain.Constants;
using Infrastructure.Factories.TokenFactories.Abstractions;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;

namespace Infrastructure.Factories.TokenFactories;
public class ResourceRegistrationAccessTokenFactory : TokenFactory
{
  public ResourceRegistrationAccessTokenFactory(
    ILogger<TokenFactory> logger, 
    IdentityConfiguration identityConfiguration, 
    IOptions<JwtBearerOptions> jwtBearerOptions, 
    JwkManager jwkManager) : base(logger, identityConfiguration, jwtBearerOptions.Value, jwkManager)
  {
  }

  public string GenerateToken(string resourceId)
  {
    var expires = DateTime.MaxValue;
    var claims = new Dictionary<string, object> 
    {
      { JwtRegisteredClaimNames.Aud, AudienceConstants.IdentityProvider },
      { ClaimNameConstants.Scope, string.Join(' ', ScopeConstants.ResourceConfiguration) },
      { ClaimNameConstants.ResourceId, resourceId }
    };
    return GetSignedToken(claims, expires);
  }
}
