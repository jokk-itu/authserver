using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Cryptography;
using System.Text;
using Application;

namespace Infrastructure.Repositories;
public class JwkManager
{
  private const int KeySize = 4096;

  private readonly IdentityContext _identityContext;
  private readonly IdentityConfiguration _identityConfiguration;

  private readonly Jwk _current;

  public RSACryptoServiceProvider RsaCryptoServiceProvider { get; }

  public IEnumerable<Jwk> Jwks
  {
    get
    {
      yield return _current;
    }
  }

  public string KeyId => _current.KeyId.ToString();

  public JwkManager(IServiceProvider serviceProvider)
  {
    var scope = serviceProvider.CreateScope();
    _identityContext = scope.ServiceProvider.GetRequiredService<IdentityContext>();
    _identityConfiguration = scope.ServiceProvider.GetRequiredService<IdentityConfiguration>();

    var jwks = GetOneJwkAsync().GetAwaiter().GetResult();

    if(jwks.Count != 1)
    {
      GenerateJwkAsync(DateTime.UtcNow).GetAwaiter().GetResult();
      jwks = GetOneJwkAsync().GetAwaiter().GetResult();
    }

    _current = jwks.ElementAt(0);

    RsaCryptoServiceProvider = new RSACryptoServiceProvider(KeySize);
    RsaCryptoServiceProvider.ImportEncryptedPkcs8PrivateKey(_identityConfiguration.PrivateKeySecret, _current.PrivateKey, out var bytesRead);

    if (bytesRead != _current.PrivateKey.Length)
      throw new CryptographicException("Private key has not been read correctly");
  }

  private async Task GenerateJwkAsync(
    DateTime createdTimeStamp,
    CancellationToken cancellationToken = default)
  {
    using var rsa = new RSACryptoServiceProvider(KeySize);
    var password = Encoding.Default.GetBytes(_identityConfiguration.PrivateKeySecret);
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
    await _identityContext.Set<Jwk>().AddAsync(jwk, cancellationToken);
    await _identityContext.SaveChangesAsync(cancellationToken);
  }

  private async Task<ICollection<Jwk>> GetOneJwkAsync()
  {
    return await _identityContext.Set<Jwk>()
      .OrderBy(jwk => jwk.CreatedTimestamp)
      .Take(1)
      .ToListAsync();
  }
}