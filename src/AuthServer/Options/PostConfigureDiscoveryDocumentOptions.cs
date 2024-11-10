using AuthServer.Constants;
using Microsoft.Extensions.Options;

namespace AuthServer.Options;

internal class PostConfigureDiscoveryDocumentOptions : IPostConfigureOptions<DiscoveryDocument>
{
	public void PostConfigure(string? name, DiscoveryDocument options)
	{
		ScopeConstants.Scopes
			.Where(x => !options.ScopesSupported.Contains(x))
			.ToList()
			.ForEach(options.ScopesSupported.Add);

		if (options.IdTokenSigningAlgValuesSupported.Count == 0)
		{
			options.IdTokenSigningAlgValuesSupported = [JwsAlgConstants.RsaSha256];
		}

		if (options.RequestObjectSigningAlgValuesSupported.Count == 0)
		{
			options.RequestObjectSigningAlgValuesSupported = [JwsAlgConstants.RsaSha256];
		}

		if (options.UserinfoSigningAlgValuesSupported.Count == 0)
		{
			options.UserinfoSigningAlgValuesSupported = [JwsAlgConstants.RsaSha256];
		}

		if (options.TokenEndpointAuthSigningAlgValuesSupported.Count == 0)
		{
			options.TokenEndpointAuthSigningAlgValuesSupported = [JwsAlgConstants.RsaSha256];
		}

		if (options.IntrospectionEndpointAuthSigningAlgValuesSupported.Count == 0)
		{
			options.IntrospectionEndpointAuthSigningAlgValuesSupported = [JwsAlgConstants.RsaSha256];
		}

		if (options.RevocationEndpointAuthSigningAlgValuesSupported.Count == 0)
		{
			options.RevocationEndpointAuthSigningAlgValuesSupported = [JwsAlgConstants.RsaSha256];
		}
	}
}