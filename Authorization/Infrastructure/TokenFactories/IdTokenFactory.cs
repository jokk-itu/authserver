using Infrastructure.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;

namespace AuthorizationServer.TokenFactories;

public class IdTokenFactory
{
  private readonly IdentityConfiguration _identityConfiguration;
  private readonly TokenValidationParameters _tokenValidationParameters;
  private readonly UserManager<IdentityUser> _userManager;
  private readonly JwkManager _jwkManager;

  public IdTokenFactory(
    IdentityConfiguration identityConfiguration,
    TokenValidationParameters tokenValidationParameters,
    UserManager<IdentityUser> userManager,
    JwkManager jwkManager)
  {
    _identityConfiguration = identityConfiguration;
    _tokenValidationParameters = tokenValidationParameters;
    _userManager = userManager;
    _jwkManager = jwkManager;
  }

  public async Task<string> GenerateTokenAsync(string clientId, IEnumerable<string> scopes,
      string nonce, string userId)
  {
    var iat = DateTimeOffset.UtcNow;
    var exp = iat + TimeSpan.FromSeconds(_identityConfiguration.IdTokenExpiration);
    var claims = new List<Claim>
    {
      new(JwtRegisteredClaimNames.Sub, userId),
      new(JwtRegisteredClaimNames.Aud, clientId),
      new(JwtRegisteredClaimNames.Iss, _identityConfiguration.InternalIssuer),
      new(JwtRegisteredClaimNames.Iat, iat.ToString()),
      new(JwtRegisteredClaimNames.Exp, exp.ToString()),
      new(JwtRegisteredClaimNames.Nbf, iat.ToString()),
      new(JwtRegisteredClaimNames.AuthTime, DateTimeOffset.Now.ToUnixTimeSeconds().ToString()),
      new(JwtRegisteredClaimNames.Nonce, nonce),
      new("scope", scopes.Aggregate((elem, acc) => $"{acc} {elem}"))
    };
    var user = await _userManager.FindByIdAsync(userId);
    var userClaims = await _userManager.GetClaimsAsync(user);
    var userRoles = await _userManager.GetRolesAsync(user);
    claims.AddRange(userClaims.Select(claim => new Claim(claim.Type, claim.Value)));
    claims.Add(new Claim("roles", JsonSerializer.Serialize(userRoles)));

    var key = new RsaSecurityKey(_jwkManager.RsaCryptoServiceProvider)
    {
      KeyId = _jwkManager.KeyId
    };
    var signingCredentials = new SigningCredentials(key, SecurityAlgorithms.RsaSha256);
    var securityToken = new JwtSecurityToken(
        claims: claims,
        signingCredentials: signingCredentials);

    var token = new JwtSecurityTokenHandler().WriteToken(securityToken);
    return await Task.FromResult(token);
  }

  public Task<JwtSecurityToken> DecodeTokenAsync(string token)
  {
    new JwtSecurityTokenHandler()
        .ValidateToken(token, _tokenValidationParameters, out var validatedToken);
    return Task.FromResult((JwtSecurityToken)validatedToken);
  }

  public async Task<bool> ValidateTokenAsync(string token)
  {
    new JwtSecurityTokenHandler()
        .ValidateToken(token, _tokenValidationParameters, out _);
    return await Task.FromResult(true);
  }
}