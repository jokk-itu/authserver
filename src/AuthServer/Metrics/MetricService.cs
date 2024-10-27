using System.Diagnostics;
using System.Diagnostics.Metrics;
using AuthServer.Extensions;
using AuthServer.Metrics.Abstractions;

namespace AuthServer.Metrics;
internal class MetricService : IMetricService, IDisposable
{
    private readonly Meter _meter;

    private readonly Counter<int> _tokenBuiltAmount;
    private readonly Counter<int> _tokenDecodedAmount;
    private readonly Counter<int> _tokenIntrospectedAmount;
    private readonly Counter<int> _tokenRevokedAmount;

    private readonly Histogram<double> _tokenBuildTime;
    private readonly Histogram<double> _tokenDecodedTime;

    private readonly Histogram<double> _clientAuthenticationTime;

    private readonly Histogram<double> _authorizeInteractionTime;

    private readonly Histogram<double> _registerGetClientTime;
    private readonly Histogram<double> _registerDeleteClientTime;
    private readonly Histogram<double> _registerUpdateClientTime;

    public MetricService(IMeterFactory meterFactory)
    {
        ActivitySource = new ActivitySource("AuthServer");
        _meter = meterFactory.Create("AuthServer");

        _tokenBuiltAmount = _meter.CreateCounter<int>("authserver.token.built.count", "The amount of tokens built.");
        _tokenDecodedAmount = _meter.CreateCounter<int>("authserver.token.decoded.count", "The amount of token decoded.");
        _tokenIntrospectedAmount = _meter.CreateCounter<int>("authserver.token.introspected.count", "The amount of tokens introspected.");
        _tokenRevokedAmount = _meter.CreateCounter<int>("authserver.token.revoked.count", "The amount of tokens revoked.");

        _tokenBuildTime = _meter.CreateHistogram<double>("authserver.token.built.duration", "The time it takes for a token to be built.");
        _tokenDecodedTime = _meter.CreateHistogram<double>("authserver.token.decoded.duration", "The time it takes for a token to be decoded.");

        _clientAuthenticationTime = _meter.CreateHistogram<double>("authserver.client.authenticated.duration", "The time it takes for a client to be authenticated.");

        _authorizeInteractionTime = _meter.CreateHistogram<double>("authserver.authorize.interaction.duration", "The time it takes to deduce the interaction during authorize");

        _registerGetClientTime = _meter.CreateHistogram<double>("authserver.register.client.get", "The time it takes to get the client");
        _registerDeleteClientTime = _meter.CreateHistogram<double>("authserver.register.client.delete", "The time it takes to delete the client");
        _registerUpdateClientTime = _meter.CreateHistogram<double>("authserver.register.client.update", "The time it takes to update the client");
    }

    public ActivitySource ActivitySource { get; }

    public void AddBuiltToken(long durationMilliseconds, TokenTypeTag tokenTypeTag, TokenStructureTag tokenStructureTag)
    {
        var tags = new[]
        {
            new KeyValuePair<string, object?>("typ", tokenTypeTag.GetDescription()),
            new KeyValuePair<string, object?>("structure", tokenStructureTag.GetDescription())
        };

        _tokenBuiltAmount.Add(1, tags);
        _tokenBuildTime.Record(durationMilliseconds, tags);
    }

    public void AddDecodedToken(long durationMilliseconds, TokenTypeTag tokenTypeTag, TokenStructureTag tokenStructureTag)
    {
        var tags = new[]
        {
            new KeyValuePair<string, object?>("typ", tokenTypeTag.GetDescription()),
            new KeyValuePair<string, object?>("structure", tokenStructureTag.GetDescription())
        };

        _tokenDecodedAmount.Add(1, tags);
        _tokenDecodedTime.Record(durationMilliseconds, tags);
    }

    public void AddIntrospectedToken(TokenTypeTag tokenTypeTag)
    {
        _tokenIntrospectedAmount.Add(1, new KeyValuePair<string, object?>("typ", tokenTypeTag.GetDescription()));
    }

    public void AddRevokedToken(TokenTypeTag tokenTypeTape)
    {
        _tokenRevokedAmount.Add(1, new KeyValuePair<string, object?>("typ", tokenTypeTape.GetDescription()));
    }

    public void AddClientAuthenticated(long durationMilliseconds, string? clientId)
    {
        _clientAuthenticationTime.Record(durationMilliseconds, new KeyValuePair<string, object?>("client_id", clientId));
    }

    public void AddAuthorizeInteraction(long durationMilliseconds, string clientId, string prompt, AuthenticationKind authenticationKind)
    {
        _authorizeInteractionTime.Record(
            durationMilliseconds,
            new KeyValuePair<string, object?>("client_id", clientId),
            new KeyValuePair<string, object?>("prompt", prompt),
            new KeyValuePair<string, object?>("authentication_kind", authenticationKind));
    }

    public void AddClientDelete(long durationMilliseconds)
    {
        _registerDeleteClientTime.Record(durationMilliseconds);
    }

    public void AddClientUpdate(long durationMilliseconds, string clientId)
    {
        _registerUpdateClientTime.Record(durationMilliseconds, new KeyValuePair<string, object?>("client_id", clientId));
    }

    public void AddClientGet(long durationMilliseconds, string clientId)
    {
        _registerGetClientTime.Record(durationMilliseconds, new KeyValuePair<string, object?>("client_id", clientId));
    }

    public void Dispose()
    {
        ActivitySource.Dispose();
        _meter.Dispose();
    }
}