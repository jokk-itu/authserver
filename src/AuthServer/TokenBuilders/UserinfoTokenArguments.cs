using AuthServer.Enums;

namespace AuthServer.TokenBuilders;
public class UserinfoTokenArguments
{
    public required string ClientId { get; init; }
    public required SigningAlg SigningAlg { get; init; }
    public EncryptionAlg? EncryptionAlg { get; init; }
    public EncryptionEnc? EncryptionEnc { get; init; }
    public required IReadOnlyDictionary<string, object> EndUserClaims { get; init; }
}