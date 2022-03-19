using System.Security.Cryptography;
using AuthorizationServer.Entities;

namespace AuthorizationServer.Repositories
{
    public class AsymmetricKeyPairManager
    {
        private readonly IdentityContext _context;
        private readonly AuthenticationConfiguration _configuration;

        public AsymmetricKeyPairManager(IdentityContext context, AuthenticationConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<AsymmetricKeyPair> FindAsymmetricKeyPairAsync(string publicKey)
        {
            var asymmetricKeyPair = await _context.KeyPairs.FindAsync(publicKey);

            if (asymmetricKeyPair is null)
                throw new Exception("key does not exist");

            return asymmetricKeyPair;
        }

        public async Task CreateAsymmetricKeyPair(string purpose, DateTimeOffset notBefore)
        {
            using var rsa = new RSACryptoServiceProvider(2048) { PersistKeyInCsp = false };
            var asymmetricKeyPair = new AsymmetricKeyPair
            {
                Purpose = purpose,
                PublicKey = rsa.ExportRSAPublicKey(),
                PrivateKey = rsa.ExportRSAPrivateKey(),
                NotBefore = notBefore,
                Expiration = notBefore.AddDays(7),
                IsRevoked = false
            };

            await _context.KeyPairs.AddAsync(asymmetricKeyPair);
            await _context.SaveChangesAsync();
        }
    }
}