using AuthServer.Enums;
using Microsoft.IdentityModel.Tokens;

namespace AuthServer.Core.Abstractions;
internal interface ITokenSecurityService
{
    Task<EncryptingCredentials?> GetEncryptingCredentials(string clientId, EncryptionAlg encryptionAlg, EncryptionEnc encryptionEnc, CancellationToken cancellationToken);
}