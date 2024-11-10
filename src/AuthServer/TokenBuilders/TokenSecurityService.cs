using AuthServer.Authentication.Abstractions;
using AuthServer.Enums;
using AuthServer.Extensions;
using AuthServer.Options;
using AuthServer.TokenBuilders.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace AuthServer.TokenBuilders;

internal class TokenSecurityService : ITokenSecurityService
{
    private readonly IClientJwkService _clientJwkService;
    private readonly IOptionsSnapshot<JwksDocument> _jwksDocumentOptions;

    private static readonly IReadOnlyCollection<EncryptionAlg> EllipticCurveAlgorithms =
        [EncryptionAlg.EcdhEsA128KW, EncryptionAlg.EcdhEsA192KW, EncryptionAlg.EcdhEsA256KW];

    public TokenSecurityService(
        IClientJwkService clientJwkService,
        IOptionsSnapshot<JwksDocument> jwksDocumentOptions)
    {
        _clientJwkService = clientJwkService;
        _jwksDocumentOptions = jwksDocumentOptions;
    }

    public async Task<EncryptingCredentials?> GetEncryptingCredentials(string clientId, EncryptionAlg encryptionAlg,
        EncryptionEnc encryptionEnc, CancellationToken cancellationToken)
    {
        var encryptionKey = await _clientJwkService.GetEncryptionKey(clientId, cancellationToken);
        if (encryptionKey is null)
        {
            return null;
        }

        var alg = encryptionAlg.GetDescription();
        var enc = encryptionEnc.GetDescription();
        if (!EllipticCurveAlgorithms.Contains(encryptionAlg))
        {
            return new EncryptingCredentials(encryptionKey, alg, enc);
        }

        var privateKey = _jwksDocumentOptions.Value.GetEncryptionKey(encryptionAlg);
        return new EncryptingCredentials(privateKey, alg, enc)
        {
            KeyExchangePublicKey = encryptionKey
        };
    }
}