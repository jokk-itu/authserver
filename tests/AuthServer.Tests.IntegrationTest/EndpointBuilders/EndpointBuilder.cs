using AuthServer.Options;
using AuthServer.Tests.Core;
using Xunit.Abstractions;

namespace AuthServer.Tests.IntegrationTest.EndpointBuilders;
public abstract class EndpointBuilder
{
    protected readonly DiscoveryDocument DiscoveryDocument;
    protected readonly JwksDocument JwksDocument;
    protected readonly HttpClient HttpClient;
    protected readonly ITestOutputHelper TestOutputHelper;
    protected readonly JwtBuilder JwtBuilder;

    protected EndpointBuilder(
        HttpClient httpClient,
        DiscoveryDocument discoveryDocument,
        JwksDocument jwksDocument,
        ITestOutputHelper testOutputHelper)
    {
        DiscoveryDocument = discoveryDocument;
        JwksDocument = jwksDocument;
        HttpClient = httpClient;
        TestOutputHelper = testOutputHelper;
        JwtBuilder = new JwtBuilder(discoveryDocument, jwksDocument);
    }
}
