using AuthorizationServer;
using Domain;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text;

namespace Infrastructure.Repositories;
public class JwkManager
{
  private static readonly int _expirationDays = 7;
  private static readonly int _keySize = 4096;

  private readonly IdentityContext _identityContext;
  private readonly IdentityConfiguration _identityConfiguration;

  private IdentityJwk _previous;
  private IdentityJwk _current;
  private IdentityJwk _future;
  private DateTimeOffset _expirationDate;

  private RSACryptoServiceProvider _rsaCryptoServiceProvider;
  public RSACryptoServiceProvider RsaCryptoServiceProvider
  {
    get
    {
      lock (_rsaCryptoServiceProvider)
      {
        if (_expirationDate.CompareTo(DateTimeOffset.UtcNow) < 0)
          RotateAsync().GetAwaiter().GetResult();

        return _rsaCryptoServiceProvider;
      }
    }
  }

  public IEnumerable<IdentityJwk> Jwks
  {
    get
    {
      lock (_rsaCryptoServiceProvider)
      {
        yield return _previous;
        yield return _current;
        yield return _future;
      }
    }
  }

  public string KeyId
  {
    get
    {
      lock (_rsaCryptoServiceProvider)
        return _current.KeyId.ToString();
    }
  }

  public JwkManager(IServiceProvider serviceProvider)
  {
    var scope = serviceProvider.CreateScope();
    _identityContext = scope.ServiceProvider.GetRequiredService<IdentityContext>();
    _identityConfiguration = scope.ServiceProvider.GetRequiredService<IdentityConfiguration>();

    var jwks = _identityContext.Jwks
      .OrderBy(jwk => jwk.CreatedTimestamp)
      .Take(3)
      .ToList();
    _previous = jwks[0];
    _current = jwks[1];
    _future = jwks[2];
    _expirationDate = _current.CreatedTimestamp.AddDays(_expirationDays);
    _rsaCryptoServiceProvider = new RSACryptoServiceProvider(_keySize);
    _rsaCryptoServiceProvider.ImportEncryptedPkcs8PrivateKey(_identityConfiguration.PrivateKeySecret, _current.PrivateKey, out var bytesRead);
    if (bytesRead != _current.PrivateKey.Length)
      throw new Exception("Privatekey has not been read correctly");
  }

  private async Task RotateAsync()
  {
    _previous = _current;
    _current = _future;
    await GenerateJwkAsync();
    var jwk = _identityContext.Jwks
      .OrderByDescending(jwk => jwk.CreatedTimestamp)
      .Last();
    _future = jwk;

    _expirationDate = _current.CreatedTimestamp.AddDays(_expirationDays);

    _rsaCryptoServiceProvider.Dispose();
    _rsaCryptoServiceProvider = new RSACryptoServiceProvider(_keySize);
    _rsaCryptoServiceProvider.ImportEncryptedPkcs8PrivateKey(_identityConfiguration.PrivateKeySecret, _current.PrivateKey, out var bytesRead);
    if (bytesRead != _current.PrivateKey.Length)
      throw new Exception("Privatekey has not been read correctly");
  }

  public async Task<bool> VerifyAsync(string token)
  {
    IdentityModelEventSource.ShowPII = true;
    var rsaParameters = RsaCryptoServiceProvider.ExportParameters(false);
    var tokenValidationParameters = new TokenValidationParameters
    {
      IssuerSigningKey = new RsaSecurityKey(rsaParameters) { KeyId = "2" },
      ValidAudience = "api1",
      ValidIssuer = "http://auth-app:80",
      ValidateLifetime = false
    };
    var claimsPrincipal = new JwtSecurityTokenHandler().ValidateToken(token, tokenValidationParameters, out var validatedToken);
    return true;
  }

  public static async Task GenerateJwkAsync(IdentityContext identityContext, IdentityConfiguration identityConfiguration, DateTimeOffset createdTimeStamp)
  {
    using var rsa = new RSACryptoServiceProvider(_keySize);
    var password = Encoding.Default.GetBytes(identityConfiguration.PrivateKeySecret);
    var pbeParameters = new PbeParameters(PbeEncryptionAlgorithm.Aes128Cbc, HashAlgorithmName.SHA256, 10);
    var privateKey = rsa.ExportEncryptedPkcs8PrivateKey(password, pbeParameters);
    var publicKey = rsa.ExportParameters(false);
    var jwk = new IdentityJwk
    {
      PrivateKey = privateKey,
      Modulus = publicKey.Modulus!,
      Exponent = publicKey.Exponent!,
      CreatedTimestamp = createdTimeStamp
    };
    await identityContext.Jwks.AddAsync(jwk);
    await identityContext.SaveChangesAsync();
  }

  public async Task GenerateJwkAsync(DateTimeOffset createdTimeStamp)
  {
    await GenerateJwkAsync(_identityContext, _identityConfiguration, createdTimeStamp);
  }

  public async Task GenerateJwkAsync()
  {
    await GenerateJwkAsync(DateTimeOffset.UtcNow);
  }
}