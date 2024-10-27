using System.Diagnostics;

namespace AuthServer.Metrics.Abstractions;
internal interface IMetricService
{
    public ActivitySource ActivitySource { get; }

    void AddBuiltToken(long durationMilliseconds, TokenTypeTag tokenTypeTag, TokenStructureTag tokenStructureTag);
    void AddDecodedToken(long durationMilliseconds, TokenTypeTag tokenTypeTag, TokenStructureTag tokenStructureTag);
    void AddIntrospectedToken(TokenTypeTag tokenTypeTag);
    void AddRevokedToken(TokenTypeTag tokenTypeTag);
    void AddClientAuthenticated(long durationMilliseconds, string? clientId);
    void AddAuthorizeInteraction(long durationMilliseconds, string clientId, string prompt, AuthenticationKind authenticationKind);
    void AddClientGet(long durationMilliseconds, string clientId);
    void AddClientUpdate(long durationMilliseconds, string clientId);
    void AddClientDelete(long durationMilliseconds);
}
