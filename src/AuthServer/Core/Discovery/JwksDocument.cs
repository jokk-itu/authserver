using AuthServer.Enums;
using AuthServer.Extensions;
using Microsoft.IdentityModel.Tokens;

namespace AuthServer.Core.Discovery;

public class JwksDocument
{
    public IReadOnlyCollection<SigningKey> SigningKeys { get; set; } = [];
    public IReadOnlyCollection<EncryptionKey> EncryptionKeys { get; set; } = [];

    /// <summary>
    /// Used at the token endpoint when signing access tokens and refresh tokens.
    /// </summary>
    public Func<SigningKey> GetTokenSigningKey { get; set; } = null!;

    public AsymmetricSecurityKey GetSigningKey(SigningAlg signingAlg)
        => SigningKeys
            .Where(x => x.Alg == signingAlg)
            .Select(x => x.Key)
            .Single();

    public AsymmetricSecurityKey GetEncryptionKey(EncryptionAlg encryptionAlg)
        => EncryptionKeys
            .Where(x => x.Alg == encryptionAlg)
            .Select(x => x.Key)
            .Single();

    public JsonWebKeySet ConvertToJwks()
    {
        // TODO remove private key from JWK
        var jwks = new JsonWebKeySet();
        foreach (var signingKey in SigningKeys)
        {
            var jsonWebKey = JsonWebKeyConverter.ConvertFromSecurityKey(signingKey.Key);
            jsonWebKey.KeyId = signingKey.Kid;
            jsonWebKey.Kid = signingKey.Kid;
            jsonWebKey.Use = JsonWebKeyUseNames.Sig;
            jsonWebKey.Alg = signingKey.Alg.GetDescription();
            jwks.Keys.Add(jsonWebKey);
        }

        foreach (var encryptionKey in EncryptionKeys)
        {
            var jsonWebKey = JsonWebKeyConverter.ConvertFromSecurityKey(encryptionKey.Key);
            jsonWebKey.KeyId = encryptionKey.Kid;
            jsonWebKey.Kid = encryptionKey.Kid;
            jsonWebKey.Use = JsonWebKeyUseNames.Enc;
            jsonWebKey.Alg = encryptionKey.Alg.GetDescription();
            jwks.Keys.Add(jsonWebKey);
        }

        return jwks;
    }

    public sealed record SigningKey(AsymmetricSecurityKey Key, SigningAlg Alg)
    {
        public string Kid { get; } = Guid.NewGuid().ToString();
    }

    public sealed record EncryptionKey(AsymmetricSecurityKey Key, EncryptionAlg Alg)
    {
        public string Kid { get; } = Guid.NewGuid().ToString();
    }
}