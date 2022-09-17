using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Cryptography;
using System.Text;

namespace Infrastructure.Repositories;
public class JwkManager
{
  private const int KeySize = 4096;

  private readonly IdentityContext _identityContext;
  private readonly IdentityConfiguration _identityConfiguration;

  private readonly Jwk _previous;
  private readonly Jwk _current;
  private readonly Jwk _future;

  public RSACryptoServiceProvider RsaCryptoServiceProvider { get; }

  public IEnumerable<Jwk> Jwks
  {
    get
    {
      yield return _previous;
      yield return _current;
      yield return _future;
    }
  }

  public string KeyId => _current.KeyId.ToString();

  public JwkManager(IServiceProvider serviceProvider)
  {
    var scope = serviceProvider.CreateScope();
    _identityContext = scope.ServiceProvider.GetRequiredService<IdentityContext>();
    _identityConfiguration = scope.ServiceProvider.GetRequiredService<IdentityConfiguration>();

    var jwks = GetThreeJwksAsync().GetAwaiter().GetResult();

    if(jwks.Count != 3)
    {
      GenerateJwkAsync(DateTime.UtcNow.AddDays(-7)).GetAwaiter().GetResult();
      GenerateJwkAsync(DateTime.UtcNow).GetAwaiter().GetResult();
      GenerateJwkAsync(DateTime.UtcNow.AddDays(7)).GetAwaiter().GetResult();
      jwks = GetThreeJwksAsync().GetAwaiter().GetResult();
    }

    _previous = jwks.ElementAt(0);
    _current = jwks.ElementAt(1);
    _future = jwks.ElementAt(2);

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

  private async Task<ICollection<Jwk>> GetThreeJwksAsync()
  {
    return await _identityContext.Set<Jwk>()
      .OrderBy(jwk => jwk.CreatedTimestamp)
      .Take(3)
      .ToListAsync();
  }
}