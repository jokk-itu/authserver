namespace Infrastructure.Builders.Abstractions;
public interface IDiscoveryBuilder
{
  DiscoveryDocument BuildDiscoveryDocument();
  Task<JwksDocument> BuildJwkDocument();
}
