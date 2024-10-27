using System.Runtime.CompilerServices;
using AuthServer.Enums;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using System.Text.Json;
using AuthServer.Extensions;

namespace AuthServer.Tests.Core;
public static class ClientJwkBuilder
{
    public static ClientJwks GetClientJwks(SigningAlg signingAlg = SigningAlg.RsaSha256, EncryptionAlg encryptionAlg = EncryptionAlg.RsaPKCS1)
    {
        var signatureKeys = signingAlg switch
        {
            SigningAlg.RsaSha256 or SigningAlg.RsaSha384 or SigningAlg.RsaSha512 => GetRsaSignatureKeys(signingAlg),
            SigningAlg.RsaSsaPssSha256 or SigningAlg.RsaSsaPssSha384 or SigningAlg.RsaSsaPssSha512 => GetRsaSignatureKeys(signingAlg),
            SigningAlg.EcdsaSha256 or SigningAlg.EcdsaSha384 or SigningAlg.EcdsaSha512 => GetEcdsaKeys(signingAlg),
            _ => throw new SwitchExpressionException(signingAlg)
        };

        var encryptionKeys = encryptionAlg switch
        {
            EncryptionAlg.RsaPKCS1 or EncryptionAlg.RsaOAEP => GetRsaEncryptionKeys(encryptionAlg),
            EncryptionAlg.EcdhEsA128KW or EncryptionAlg.EcdhEsA192KW or EncryptionAlg.EcdhEsA256KW => GetEcdhKeys(encryptionAlg),
            _ => throw new SwitchExpressionException(encryptionAlg)
        };

        var publicKeys = new JsonWebKeySet();
        var privateKeys = new JsonWebKeySet();

        publicKeys.Keys.Add(signatureKeys.PublicKey);
        publicKeys.Keys.Add(encryptionKeys.PublicKey);

        privateKeys.Keys.Add(signatureKeys.PrivateKey);
        privateKeys.Keys.Add(encryptionKeys.PrivateKey);
        return new ClientJwks(JsonSerializer.Serialize(publicKeys), JsonSerializer.Serialize(privateKeys));
    }

    private static ClientKeys GetRsaSignatureKeys(SigningAlg signingAlg)
    {
        var signingKeyId = Guid.NewGuid().ToString();
        var signingRsa = RSA.Create(3072);

        var privateKey = GetRsaSigningKey(signingRsa, signingKeyId, signingAlg, true);
        var publicKey = GetRsaSigningKey(signingRsa, signingKeyId, signingAlg, false);
        return new ClientKeys(publicKey, privateKey);
    }

    private static ClientKeys GetRsaEncryptionKeys(EncryptionAlg encryptionAlg)
    {
        var encryptionKeyId = Guid.NewGuid().ToString();
        var encryptionRsa = RSA.Create(3072);

        var privateKey = GetRsaEncryptionKey(encryptionRsa, encryptionKeyId, encryptionAlg, true);
        var publicKey = GetRsaEncryptionKey(encryptionRsa, encryptionKeyId, encryptionAlg, false);
        return new ClientKeys(publicKey, privateKey);
    }

    private static ClientKeys GetEcdsaKeys(SigningAlg signingAlg)
    {
        var signingKeyId = Guid.NewGuid().ToString();
        var ecdsa = ECDsa.Create();

        var privateKey = GetEcdsaSigningKey(ecdsa, signingKeyId, signingAlg, true);
        var publicKey = GetEcdsaSigningKey(ecdsa, signingKeyId, signingAlg, false);
        return new ClientKeys(publicKey, privateKey);
    }

    private static ClientKeys GetEcdhKeys(EncryptionAlg encryptionAlg)
    {
        var encryptionKeyId = Guid.NewGuid().ToString();
        var ecdsa = ECDsa.Create();

        var privateKey = GetEcdhEncryptionKey(ecdsa, encryptionKeyId, encryptionAlg, true);
        var publicKey = GetEcdhEncryptionKey(ecdsa, encryptionKeyId, encryptionAlg, false);
        return new ClientKeys(publicKey, privateKey);
    }

    private static JsonWebKey GetRsaSigningKey(RSA rsa, string keyId, SigningAlg signingAlg, bool exportPrivateKey)
    {
        var signingKey = new RsaSecurityKey(rsa.ExportParameters(exportPrivateKey));
        var signingJwk = JsonWebKeyConverter.ConvertFromRSASecurityKey(signingKey);
        signingJwk.Use = JsonWebKeyUseNames.Sig;
        signingJwk.Alg = signingAlg.GetDescription();
        signingJwk.KeyId = keyId;
        return signingJwk;
    }

    private static JsonWebKey GetRsaEncryptionKey(RSA rsa, string keyId, EncryptionAlg encryptionAlg, bool exportPrivateKey)
    {
        var encryptionKey = new RsaSecurityKey(rsa.ExportParameters(exportPrivateKey));
        var encryptionJwk = JsonWebKeyConverter.ConvertFromRSASecurityKey(encryptionKey);
        encryptionJwk.Use = JsonWebKeyUseNames.Enc;
        encryptionJwk.Alg = encryptionAlg.GetDescription();
        encryptionJwk.KeyId = keyId;
        return encryptionJwk;
    }

    private static JsonWebKey GetEcdsaSigningKey(ECDsa ecdsa, string keyId, SigningAlg signingAlg,
        bool exportPrivateKey)
    {
        var signingKey = new ECDsaSecurityKey(ECDsa.Create(ecdsa.ExportParameters(exportPrivateKey)));
        var signingJwk = JsonWebKeyConverter.ConvertFromECDsaSecurityKey(signingKey);
        signingJwk.Use = JsonWebKeyUseNames.Sig;
        signingJwk.Alg = signingAlg.GetDescription();
        signingJwk.KeyId = keyId;
        return signingJwk;
    }

    private static JsonWebKey GetEcdhEncryptionKey(ECDsa ecdsa, string keyId, EncryptionAlg encryptionAlg,
        bool exportPrivateKey)
    {
        var encryptionKey = new ECDsaSecurityKey(ECDsa.Create(ecdsa.ExportParameters(exportPrivateKey)));
        var encryptionJwk = JsonWebKeyConverter.ConvertFromECDsaSecurityKey(encryptionKey);
        encryptionJwk.Use = JsonWebKeyUseNames.Enc;
        encryptionJwk.Alg = encryptionAlg.GetDescription();
        encryptionJwk.KeyId = keyId;
        return encryptionJwk;
    }

    public sealed record ClientJwks(string PublicJwks, string PrivateJwks);

    private sealed record ClientKeys(JsonWebKey PublicKey, JsonWebKey PrivateKey);
}