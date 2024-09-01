using AuthServer.Enums;
using Microsoft.IdentityModel.Tokens;

namespace AuthServer.TokenBuilders.Abstractions;
internal interface ITokenSecurityService
{
    Task<EncryptingCredentials?> GetEncryptingCredentials(string clientId, EncryptionAlg encryptionAlg, EncryptionEnc encryptionEnc, CancellationToken cancellationToken);
}