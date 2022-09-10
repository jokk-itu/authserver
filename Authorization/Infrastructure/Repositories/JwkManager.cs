using Domain;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Cryptography;
using System.Text;

namespace Infrastructure.Repositories;
public class JwkManager
{
  private static readonly int _expirationDays = 7;
  private static readonly int _keySize = 4096;

  private readonly IdentityContext _identityContext;
  private readonly IdentityConfiguration _identityConfiguration;

  private Jwk _previous;
  private Jwk _current;
  private Jwk _future;
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

  public IEnumerable<Jwk> Jwks
  {
    get
    {
      lock (_rsaCryptoServiceProvider)
      {
        if (_expirationDate.CompareTo(DateTimeOffset.UtcNow) < 0)
          RotateAsync().GetAwaiter().GetResult();

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

    var jwks = _identityContext.Set<Jwk>()
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
      throw new CryptographicException("Private key has not been read correctly");
  }

  private async Task RotateAsync(CancellationToken cancellationToken = default)
  {
    _previous = _current;
    _current = _future;
    await GenerateJwkAsync(cancellationToken);
    var jwk = _identityContext.Set<Jwk>()
      .OrderByDescending(jwk => jwk.CreatedTimestamp)
      .Last();

    _future = jwk;

    _expirationDate = _current.CreatedTimestamp.AddDays(_expirationDays);

    _rsaCryptoServiceProvider.Dispose();
    _rsaCryptoServiceProvider = new RSACryptoServiceProvider(_keySize);
    _rsaCryptoServiceProvider.ImportEncryptedPkcs8PrivateKey(_identityConfiguration.PrivateKeySecret, _current.PrivateKey, out var bytesRead);
    if (bytesRead != _current.PrivateKey.Length)
      throw new CryptographicException("Private key has not been read correctly");
  }

  public static async Task GenerateJwkAsync(
    IdentityContext identityContext,
    IdentityConfiguration identityConfiguration,
    DateTime createdTimeStamp,
    CancellationToken cancellationToken = default)
  {
    using var rsa = new RSACryptoServiceProvider(_keySize);
    var password = Encoding.Default.GetBytes(identityConfiguration.PrivateKeySecret);
    var pbeParameters = new PbeParameters(PbeEncryptionAlgorithm.Aes128Cbc, HashAlgorithmName.SHA256, 10);
    var privateKey = rsa.ExportEncryptedPkcs8PrivateKey(password, pbeParameters);
    var publicKey = rsa.ExportParameters(false);
    var jwk = new Jwk
    {
      PrivateKey = privateKey,
      Modulus = publicKey.Modulus!,
      Exponent = publicKey.Exponent!,
      CreatedTimestamp = createdTimeStamp
    };
    await identityContext.Set<Jwk>().AddAsync(jwk, cancellationToken);
    await identityContext.SaveChangesAsync(cancellationToken);
  }

  public async Task GenerateJwkAsync(DateTime createdTimeStamp, CancellationToken cancellationToken = default)
  {
    await GenerateJwkAsync(_identityContext, _identityConfiguration, createdTimeStamp, cancellationToken);
  }

  public async Task GenerateJwkAsync(CancellationToken cancellationToken = default)
  {
    await GenerateJwkAsync(DateTime.UtcNow, cancellationToken: cancellationToken);
  }
}