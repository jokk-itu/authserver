using AuthServer.Constants;
using Microsoft.Extensions.Options;

namespace AuthServer.Options;

public class ValidateDiscoveryDocumentOptions : IValidateOptions<DiscoveryDocument>
{
    public ValidateOptionsResult Validate(string? name, DiscoveryDocument options)
    {
        if (string.IsNullOrEmpty(options.Issuer))
        {
            return ValidateOptionsResult.Fail($"{nameof(options.Issuer)} is not specified");
        }

        if (!Uri.IsWellFormedUriString(options.Issuer, UriKind.Absolute))
        {
            return ValidateOptionsResult.Fail($"{nameof(options.Issuer)} is not an absolute well formed uri");
        }

        if (!string.IsNullOrEmpty(options.ServiceDocumentation) && !Uri.IsWellFormedUriString(options.ServiceDocumentation, UriKind.Absolute))
        {
            return ValidateOptionsResult.Fail($"{nameof(options.ServiceDocumentation)} is not an absolute well formed uri");
        }

        if (!string.IsNullOrEmpty(options.OpPolicyUri) && !Uri.IsWellFormedUriString(options.OpPolicyUri, UriKind.Absolute))
        {
            return ValidateOptionsResult.Fail($"{nameof(options.OpPolicyUri)} is not an absolute well formed uri");
        }

        if (!string.IsNullOrEmpty(options.OpTosUri) && !Uri.IsWellFormedUriString(options.OpTosUri, UriKind.Absolute))
        {
            return ValidateOptionsResult.Fail($"{nameof(options.OpTosUri)} is not an absolute well formed uri");
        }

        if (options.ClaimsSupported.Count == 0)
        {
            return ValidateOptionsResult.Fail($"{nameof(options.ClaimsSupported)} is not specified");
        }

        if (options.ScopesSupported.Count == 0)
        {
            return ValidateOptionsResult.Fail($"{nameof(options.ScopesSupported)} is not specified");
        }

        var invalidTokenEndpointAuthSigningAlgValues =
            options.TokenEndpointAuthSigningAlgValuesSupported.Where(x => !JwsAlgConstants.AlgValues.Contains(x)).ToList();
        if (invalidTokenEndpointAuthSigningAlgValues.Count != 0)
        {
            return ValidateOptionsResult.Fail($"{nameof(options.TokenEndpointAuthSigningAlgValuesSupported)} have unrecognized values: {string.Join(',', invalidTokenEndpointAuthSigningAlgValues)}");
        }

        var invalidIdTokenSigningAlgValues =
            options.IdTokenSigningAlgValuesSupported.Where(x => !JwsAlgConstants.AlgValues.Contains(x)).ToList();
        if (invalidIdTokenSigningAlgValues.Count != 0)
        {
            return ValidateOptionsResult.Fail($"{nameof(options.IdTokenSigningAlgValuesSupported)} have unrecognized values: {string.Join(',', invalidIdTokenSigningAlgValues)}");
        }

        var invalidIdTokenEncryptionAlgValues =
            options.IdTokenEncryptionAlgValuesSupported.Where(x => !JweAlgConstants.AlgValues.Contains(x)).ToList();
        if (invalidIdTokenEncryptionAlgValues.Count != 0)
        {
            return ValidateOptionsResult.Fail($"{nameof(options.IdTokenEncryptionAlgValuesSupported)} have unrecognized values: {string.Join(',', invalidIdTokenEncryptionAlgValues)}");
        }

        var invalidIdTokenEncryptionEncValues =
            options.IdTokenEncryptionEncValuesSupported.Where(x => !JweEncConstants.EncValues.Contains(x)).ToList();
        if (invalidIdTokenEncryptionEncValues.Count != 0)
        {
            return ValidateOptionsResult.Fail($"{nameof(options.IdTokenEncryptionEncValuesSupported)} have unrecognized values: {string.Join(',', invalidIdTokenEncryptionEncValues)}");
        }

        var invalidUserinfoSigningAlgValues =
            options.UserinfoSigningAlgValuesSupported.Where(x => !JwsAlgConstants.AlgValues.Contains(x)).ToList();
        if (invalidUserinfoSigningAlgValues.Count != 0)
        {
            return ValidateOptionsResult.Fail($"{nameof(options.UserinfoSigningAlgValuesSupported)} have unrecognized values: {string.Join(',', invalidUserinfoSigningAlgValues)}");
        }

        var invalidUserinfoEncryptionAlgValues =
            options.UserinfoEncryptionAlgValuesSupported.Where(x => !JweAlgConstants.AlgValues.Contains(x)).ToList();
        if (invalidUserinfoEncryptionAlgValues.Count != 0)
        {
            return ValidateOptionsResult.Fail($"{nameof(options.UserinfoEncryptionAlgValuesSupported)} have unrecognized values: {string.Join(',', invalidUserinfoEncryptionAlgValues)}");
        }

        var invalidUserinfoEncryptionEncValues =
            options.UserinfoEncryptionEncValuesSupported.Where(x => !JweEncConstants.EncValues.Contains(x)).ToList();
        if (invalidUserinfoEncryptionEncValues.Count != 0)
        {
            return ValidateOptionsResult.Fail($"{nameof(options.UserinfoEncryptionEncValuesSupported)} have unrecognized values: {string.Join(',', invalidUserinfoEncryptionEncValues)}");
        }

        var invalidRevocationSigningAlgValues =
            options.RevocationEndpointAuthSigningAlgValuesSupported.Where(x => !JwsAlgConstants.AlgValues.Contains(x)).ToList();
        if (invalidRevocationSigningAlgValues.Count != 0)
        {
            return ValidateOptionsResult.Fail($"{nameof(options.RevocationEndpointAuthSigningAlgValuesSupported)} have unrecognized values: {string.Join(',', invalidRevocationSigningAlgValues)}");
        }

        var invalidIntrospectionSigningAlgValues =
            options.IntrospectionEndpointAuthSigningAlgValuesSupported.Where(x => !JwsAlgConstants.AlgValues.Contains(x)).ToList();
        if (invalidIntrospectionSigningAlgValues.Count != 0)
        {
            return ValidateOptionsResult.Fail($"{nameof(options.IntrospectionEndpointAuthSigningAlgValuesSupported)} have unrecognized values: {string.Join(',', invalidIntrospectionSigningAlgValues)}");
        }

        var invalidRequestObjectSigningAlgValues =
            options.RequestObjectSigningAlgValuesSupported.Where(x => !JwsAlgConstants.AlgValues.Contains(x)).ToList();
        if (invalidRequestObjectSigningAlgValues.Count != 0)
        {
            return ValidateOptionsResult.Fail($"{nameof(options.RequestObjectSigningAlgValuesSupported)} have unrecognized values: {string.Join(',', invalidRequestObjectSigningAlgValues)}");
        }

        var invalidRequestObjectEncryptionAlgValues =
            options.RequestObjectEncryptionAlgValuesSupported.Where(x => !JweAlgConstants.AlgValues.Contains(x)).ToList();
        if (invalidRequestObjectEncryptionAlgValues.Count != 0)
        {
            return ValidateOptionsResult.Fail($"{nameof(options.RequestObjectEncryptionAlgValuesSupported)} have unrecognized values: {string.Join(',', invalidRequestObjectEncryptionAlgValues)}");
        }

        var invalidRequestObjectEncryptionEncValues =
            options.RequestObjectEncryptionEncValuesSupported.Where(x => !JweEncConstants.EncValues.Contains(x)).ToList();
        if (invalidRequestObjectEncryptionEncValues.Count != 0)
        {
            return ValidateOptionsResult.Fail($"{nameof(options.RequestObjectEncryptionEncValuesSupported)} have unrecognized values: {string.Join(',', invalidRequestObjectEncryptionEncValues)}");
        }

		return ValidateOptionsResult.Success;
    }
}