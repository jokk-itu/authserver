using AuthServer.Constants;
using AuthServer.Core.Discovery;
using Microsoft.Extensions.Options;

namespace AuthServer.Options;
internal class ConfigureDiscoveryDocumentOptions : IPostConfigureOptions<DiscoveryDocument>
{
    public void PostConfigure(string? name, DiscoveryDocument options)
    {
        ScopeConstants.Scopes.ToList().ForEach(options.ScopesSupported.Add);
    }
}