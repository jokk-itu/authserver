using Infrastructure.Factories.TokenFactories.Abstractions;
using Infrastructure.Repositories;
using Microsoft.Extensions.Logging;
using System.IdentityModel.Tokens.Jwt;
using Domain.Constants;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Infrastructure.Factories.TokenFactories;

public class IdTokenFactory : TokenFactory
{
  public IdTokenFactory(
    IdentityConfiguration identityConfiguration,
    IOptionsMonitor<JwtBearerOptions> jwtBearerOptions,
    JwkManager jwkManager,
    ILogger<IdTokenFactory> logger)
    : base(logger, identityConfiguration, jwtBearerOptions.Get(OpenIdConnectDefaults.AuthenticationScheme), jwkManager)
  {
  }

  public async Task<string> GenerateTokenAsync(string clientId, IEnumerable<string> scopes,
      string nonce, string userId, CancellationToken cancellationToken = default)
  {
    var expires = DateTime.Now + TimeSpan.FromSeconds(_identityConfiguration.IdTokenExpiration);
    var audiences = new string[] { clientId};
    var claims = new Dictionary<string, object>
    {
      { JwtRegisteredClaimNames.Sub, userId },
      { JwtRegisteredClaimNames.Aud, audiences },
      { ClaimNameConstants.Scope, string.Join(' ', scopes) },
      { ClaimNameConstants.ClientId, clientId },
      { JwtRegisteredClaimNames.Nonce, nonce }
    };
    return GetSignedToken(claims, expires);
  }
}