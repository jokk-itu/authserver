using System.Security.Cryptography;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;

IdentityModelEventSource.ShowPII = true;
IdentityModelEventSource.LogCompleteSecurityArtifact = true;

var serverEcdsa = ECDsa.Create();
var serverPublicKey = ECDsa.Create(serverEcdsa.ExportParameters(false));
var serverJsonWebKeyEnc = JsonWebKeyConverter.ConvertFromECDsaSecurityKey(new ECDsaSecurityKey(serverPublicKey));
serverJsonWebKeyEnc.Use = JsonWebKeyUseNames.Enc;
serverJsonWebKeyEnc.Alg = SecurityAlgorithms.EcdhEsA128kw;
serverJsonWebKeyEnc.Kid = Guid.NewGuid().ToString();

var serverJsonWebKeySig = JsonWebKeyConverter.ConvertFromECDsaSecurityKey(new ECDsaSecurityKey(serverPublicKey));
serverJsonWebKeySig.Use = JsonWebKeyUseNames.Sig;
serverJsonWebKeySig.Alg = SecurityAlgorithms.EcdsaSha256;
serverJsonWebKeySig.Kid = Guid.NewGuid().ToString();
var serverIssuer = "https://idp.authserver.dk";

var clientEcdsa = ECDsa.Create();
var clientPublicKey = ECDsa.Create(clientEcdsa.ExportParameters(false));
var clientJsonWebKeyEnc = JsonWebKeyConverter.ConvertFromECDsaSecurityKey(new ECDsaSecurityKey(clientPublicKey));
clientJsonWebKeyEnc.Use = JsonWebKeyUseNames.Enc;
clientJsonWebKeyEnc.Alg = SecurityAlgorithms.EcdhEsA128kw;
clientJsonWebKeyEnc.Kid = Guid.NewGuid().ToString();

var clientJsonWebKeySig = JsonWebKeyConverter.ConvertFromECDsaSecurityKey(new ECDsaSecurityKey(clientPublicKey));
clientJsonWebKeySig.Use = JsonWebKeyUseNames.Sig;
clientJsonWebKeySig.Alg = SecurityAlgorithms.EcdsaSha256;
clientJsonWebKeySig.Kid = Guid.NewGuid().ToString();
var clientId = Guid.NewGuid().ToString();


// Client
var signingCredentials = new SigningCredentials(new ECDsaSecurityKey(clientEcdsa), SecurityAlgorithms.EcdsaSha256);

var encryptionKey = new ECDsaSecurityKey(clientEcdsa);
var encryptingCredentials = new EncryptingCredentials(
    encryptionKey, SecurityAlgorithms.EcdhEsA128kw, SecurityAlgorithms.Aes128CbcHmacSha256)
{
    KeyExchangePublicKey = serverJsonWebKeyEnc
};

var claims = new Dictionary<string, object>
{
    { "aud", serverIssuer }
};
var tokenDescriptor = new SecurityTokenDescriptor
{
    Issuer = clientId,
    SigningCredentials = signingCredentials,
    EncryptingCredentials = encryptingCredentials,
    Claims = claims
};
var tokenHandler = new JsonWebTokenHandler();
var privateKeyJwt = tokenHandler.CreateToken(tokenDescriptor);

// Server
var tokenValidationParameters = new TokenValidationParameters
{
    ValidIssuer = clientId,
    ValidAudience = serverIssuer,
    IssuerSigningKey = clientJsonWebKeySig,
    TokenDecryptionKeyResolver = (_, _, _, _) => [ new ECDsaSecurityKey(serverEcdsa) ],
    TokenDecryptionKey = new ECDsaSecurityKey(clientPublicKey) { KeyId = clientJsonWebKeyEnc.KeyId }
};
var decryptedToken = await new JsonWebTokenHandler().ValidateTokenAsync(privateKeyJwt, tokenValidationParameters);
Console.WriteLine(decryptedToken.IsValid);