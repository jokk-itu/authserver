using AuthServer.Enums;
using Microsoft.IdentityModel.Tokens;

namespace AuthServer.TokenBuilders.Abstractions;
internal interface ITokenSecurityService
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="clientId"></param>
    /// <param name="encryptionAlg"></param>
    /// <param name="encryptionEnc"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<EncryptingCredentials?> GetEncryptingCredentials(string clientId, EncryptionAlg encryptionAlg, EncryptionEnc encryptionEnc, CancellationToken cancellationToken);
}