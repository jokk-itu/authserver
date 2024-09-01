using Microsoft.Extensions.Options;

namespace AuthServer.Options;
internal class ValidateJwksDocument : IValidateOptions<JwksDocument>
{
    public ValidateOptionsResult Validate(string? name, JwksDocument options)
    {
        var hasSigningKeys = options.SigningKeys?.Count > 0;
        if (!hasSigningKeys)
        {
            ValidateOptionsResult.Fail("Missing signing keys");
        }

        var hasAllSigningKeysKid = options.SigningKeys?.All(x => string.IsNullOrEmpty(x.Key.KeyId));

        var hasEncryptionKeys = options.EncryptionKeys?.Count > 0;
        if (!hasEncryptionKeys)
        {
            ValidateOptionsResult.Fail("Missing encryption keys");
        }

        var hasEncryptionKeysKid = options.EncryptionKeys?.All(x => string.IsNullOrEmpty(x.Key.KeyId));

        return ValidateOptionsResult.Success;
    }
}
