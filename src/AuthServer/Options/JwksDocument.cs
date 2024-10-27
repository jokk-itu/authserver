using AuthServer.Enums;
using Microsoft.IdentityModel.Tokens;

namespace AuthServer.Options;

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

    public sealed record SigningKey(AsymmetricSecurityKey Key, SigningAlg Alg);
    public sealed record EncryptionKey(AsymmetricSecurityKey Key, EncryptionAlg Alg);
}