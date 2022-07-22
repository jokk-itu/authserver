using AuthorizationServer;
using Domain;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using System.Text;

namespace Infrastructure.Repositories;
public class JwkManager
{
  private readonly IdentityContext _identityContext;
  private readonly IdentityConfiguration _identityConfiguration;

  public JwkManager(IdentityContext identityContext, IdentityConfiguration identityConfiguration)
  {
    _identityContext = identityContext;
    _identityConfiguration = identityConfiguration;
  }

  public async Task<IdentityJwk> GetJwkAsync() 
  {
    var jwk = _identityContext.Jwks.FirstOrDefault();
    if (jwk is null)
      throw new Exception("There is no Jwk");

    return jwk;
  }

  public async Task<RSACryptoServiceProvider> GetRsaCryptoServiceProviderAsync(IdentityJwk jwk) 
  {
    var rsa = new RSACryptoServiceProvider(4096);
    var password = Encoding.Default.GetBytes(_identityConfiguration.PrivateKeySecret);
    rsa.ImportEncryptedPkcs8PrivateKey(password, jwk.PrivateKey, out var bytesRead);
    if (jwk.PrivateKey.Length != bytesRead)
      throw new Exception("Private key has not been read correctly");

    return rsa;
  }

  public async Task GenerateJwkAsync()
  {
    using var rsa = new RSACryptoServiceProvider(4096);
    var encodedPassword = Encoding.Default.GetBytes(_identityConfiguration.PrivateKeySecret);
    var pbeParameters = new PbeParameters(PbeEncryptionAlgorithm.Aes128Cbc, HashAlgorithmName.SHA256, 10);
    var privateKey = rsa.ExportEncryptedPkcs8PrivateKey(encodedPassword, pbeParameters);
    var jwk = new IdentityJwk
    {
      PrivateKey = privateKey
    };
    await _identityContext.Jwks.AddAsync(jwk);
    await _identityContext.SaveChangesAsync();
  }

  public async Task<RSAParameters> GetPublicKeyAsync(IdentityJwk jwk)
  {
    using var rsa = new RSACryptoServiceProvider(4096);
    rsa.ImportEncryptedPkcs8PrivateKey(Encoding.Default.GetBytes(_identityConfiguration.PrivateKeySecret), jwk.PrivateKey, out var bytesRead);
    if (jwk.PrivateKey.Length != bytesRead)
      throw new Exception("Private key has not been read correctly");

    return rsa.ExportParameters(false);
  }
}
