using Domain;
using Infrastructure.Factories.TokenFactories.Abstractions;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using Domain.Constants;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Infrastructure.Factories.TokenFactories;

public class IdTokenFactory : TokenFactory
{
  private readonly UserManager<User> _userManager;

  public IdTokenFactory(
    IdentityConfiguration identityConfiguration,
    IOptionsMonitor<JwtBearerOptions> jwtBearerOptions,
    UserManager<User> userManager,
    JwkManager jwkManager,
    ILogger<IdTokenFactory> logger)
    : base(logger, identityConfiguration, jwtBearerOptions.Get(OpenIdConnectDefaults.AuthenticationScheme), jwkManager)
  {
    _userManager = userManager;
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