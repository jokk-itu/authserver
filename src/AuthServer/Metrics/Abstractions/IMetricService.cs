using System.Diagnostics;

namespace AuthServer.Metrics.Abstractions;
internal interface IMetricService
{
    public ActivitySource ActivitySource { get; }

    void AddBuiltToken(long durationMilliseconds, TokenTypeTag tokenTypeTag, TokenStructureTag tokenStructureTag);
    void AddDecodedToken(long durationMilliseconds, TokenTypeTag tokenTypeTag, TokenStructureTag tokenStructureTag);
    void AddIntrospectedToken(TokenTypeTag tokenTypeTag);
    void AddRevokedToken(TokenTypeTag tokenTypeTag);
    void AddClientAuthenticated(long durationMilliseconds, string clientId);
}
