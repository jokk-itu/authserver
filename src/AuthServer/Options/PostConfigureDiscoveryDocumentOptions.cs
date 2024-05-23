using AuthServer.Constants;
using AuthServer.Core.Discovery;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace AuthServer.Options;

internal class PostConfigureDiscoveryDocumentOptions : IPostConfigureOptions<DiscoveryDocument>
{
    public void PostConfigure(string? name, DiscoveryDocument options)
    {
        ScopeConstants.Scopes
            .Where(x => !options.ScopesSupported.Contains(x))
            .ToList()
            .ForEach(options.ScopesSupported.Add);

        if (options.IdTokenSigningAlgValuesSupported.IsNullOrEmpty())
        {
            options.IdTokenSigningAlgValuesSupported = [JwsAlgConstants.RsaSha256];
        }

        if (options.TokenEndpointAuthSigningAlgValuesSupported.IsNullOrEmpty())
        {
            options.TokenEndpointAuthSigningAlgValuesSupported = [JwsAlgConstants.RsaSha256];
        }

        if (options.IntrospectionEndpointAuthSigningAlgValuesSupported.IsNullOrEmpty())
        {
            options.IntrospectionEndpointAuthSigningAlgValuesSupported = [JwsAlgConstants.RsaSha256];
        }

        if (options.RevocationEndpointAuthSigningAlgValuesSupported.IsNullOrEmpty())
        {
            options.RevocationEndpointAuthSigningAlgValuesSupported = [JwsAlgConstants.RsaSha256];
        }
    }
}