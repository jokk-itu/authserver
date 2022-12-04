using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace Infrastructure.Builders.Abstractions;
public interface IDiscoveryBuilder
{
  Task<DiscoveryDocument> BuildDiscoveryDocument();
  Task<JwksDocument> BuildJwkDocument();
}
