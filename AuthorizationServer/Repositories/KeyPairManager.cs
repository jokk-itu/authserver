using AuthorizationServer.Entities;

namespace AuthorizationServer.Repositories
{
  public class KeyPairManager
  {
    private readonly IdentityContext _context;

    public KeyPairManager(IdentityContext context)
    {
      _context = context;
    }

    public async Task<RSAKeyPair> FindKeyPairAsync(string publicKey) 
    {
      throw new NotImplementedException();
    }

    public async Task CreateKeyPair(string publicKey, string privateKey, DateTime notBefore) 
    {
      throw new NotImplementedException(); 
    }
  }
}
