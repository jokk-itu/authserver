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

    public void AddClientAuthenticated(long durationMilliseconds, string clientId)
    {
        _clientAuthenticationTime.Record(durationMilliseconds, new KeyValuePair<string, object?>("client_id", clientId));
    }

    public void Dispose()
    {
        ActivitySource.Dispose();
        _meter.Dispose();
    }
}