using AuthorizationServer.Repositories;
using Infrastructure.Repositories;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace AuthorizationServer.TokenFactories;

public class AccessTokenFactory
{
  private readonly IdentityConfiguration _identityConfiguration;
  private readonly TokenValidationParameters _tokenValidationParameters;
  private readonly ResourceManager _resourceManager;
  private readonly JwkManager _jwkManager;

  public AccessTokenFactory(
      IdentityConfiguration identityConfiguration,
      TokenValidationParameters tokenValidationParameters,
      ResourceManager resourceManager,
      JwkManager jwkManager)
  {
    _identityConfiguration = identityConfiguration;
    _tokenValidationParameters = tokenValidationParameters;
    _resourceManager = resourceManager;
    _jwkManager = jwkManager;
  }

  public async Task<string> GenerateTokenAsync(string clientId, ICollection<string> scopes,
      string userId)
  {
    var iat = DateTimeOffset.UtcNow;
    var exp = iat + TimeSpan.FromSeconds(_identityConfiguration.AccessTokenExpiration);
    var resources = await _resourceManager.FindResourcesByScopes(scopes);
    var aud = resources.Aggregate(string.Empty, (acc, r) => $"{acc} {r.Id}");
    var claims = new[]
    {
      new Claim(JwtRegisteredClaimNames.Sub, userId),
      new Claim(JwtRegisteredClaimNames.Aud, aud),
      new Claim(JwtRegisteredClaimNames.Iss, _identityConfiguration.InternalIssuer),
      new Claim(JwtRegisteredClaimNames.Iat, iat.ToString()),
      new Claim(JwtRegisteredClaimNames.Exp, exp.ToString()),
      new Claim(JwtRegisteredClaimNames.Nbf, iat.ToString()),
      new Claim("scope", scopes.Aggregate((elem, acc) => $"{acc} {elem}")),
      new Claim("client_id", clientId)
    };

    using var rsa = new RSACryptoServiceProvider(4096);
    var jwk = await _jwkManager.GetJwkAsync();
    var password = Encoding.Default.GetBytes(_identityConfiguration.PrivateKeySecret);
    rsa.ImportEncryptedPkcs8PrivateKey(password, jwk.PrivateKey, out var bytesRead);
    var key = new RsaSecurityKey(rsa)
    {
      KeyId = jwk.KeyId.ToString()
    };
    var signingCredentials = new SigningCredentials(key, SecurityAlgorithms.RsaSha256)
    {
      CryptoProviderFactory = new CryptoProviderFactory { CacheSignatureProviders = false }
    };
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