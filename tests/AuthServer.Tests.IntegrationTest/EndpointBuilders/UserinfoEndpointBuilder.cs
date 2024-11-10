using System.Net.Http.Headers;
using AuthServer.Options;
using Xunit.Abstractions;

namespace AuthServer.Tests.IntegrationTest.EndpointBuilders;

public class UserinfoEndpointBuilder : EndpointBuilder
{
    private string _token;

    public UserinfoEndpointBuilder(
        HttpClient httpClient,
        DiscoveryDocument discoveryDocument,
        JwksDocument jwksDocument,
        ITestOutputHelper testOutputHelper)
        : base(httpClient, discoveryDocument, jwksDocument, testOutputHelper)
    {
    }

    public UserinfoEndpointBuilder WithAccessToken(string token)
    {
        _token = token;
        return this;
    }

    internal async Task<string> Get()
    {
        var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "connect/userinfo");
        httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);
        var httpResponseMessage = await HttpClient.SendAsync(httpRequestMessage);
        var content = await httpResponseMessage.Content.ReadAsStringAsync();

        TestOutputHelper.WriteLine(
            "Received Userinfo response {0}, Content: {1}",
            httpResponseMessage.StatusCode,
            content);

        httpResponseMessage.EnsureSuccessStatusCode();
        return content;
    }

    internal async Task<string> Post()
    {
        var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, "connect/userinfo");
        httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);
        var httpResponseMessage = await HttpClient.SendAsync(httpRequestMessage);
        var content = await httpResponseMessage.Content.ReadAsStringAsync();

        TestOutputHelper.WriteLine(
            "Received Userinfo response {0}, Content: {1}",
            httpResponseMessage.StatusCode,
            content);

        httpResponseMessage.EnsureSuccessStatusCode();
        return content;
    }
}