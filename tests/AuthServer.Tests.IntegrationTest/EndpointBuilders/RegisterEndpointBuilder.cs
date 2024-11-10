using AuthServer.Constants;
using AuthServer.Core;
using AuthServer.Endpoints.Responses;
using AuthServer.Options;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using AuthServer.Enums;
using AuthServer.Extensions;
using Xunit.Abstractions;

namespace AuthServer.Tests.IntegrationTest.EndpointBuilders;

public class RegisterEndpointBuilder : EndpointBuilder
{
    private Dictionary<string, object> _parameters = [];

    public RegisterEndpointBuilder(
        HttpClient httpClient,
        DiscoveryDocument discoveryDocument,
        JwksDocument jwksDocument,
        ITestOutputHelper testOutputHelper)
        : base(httpClient, discoveryDocument, jwksDocument, testOutputHelper)
    {
    }

    public RegisterEndpointBuilder WithUserinfoSignedResponseAlg(SigningAlg signingAlg)
    {
        _parameters.Add(Parameter.UserinfoSignedResponseAlg, signingAlg.GetDescription());
        return this;
    }

    public RegisterEndpointBuilder WithSectorIdentifierUri(string sectorIdentifierUri)
    {
        _parameters.Add(Parameter.SectorIdentifierUri, sectorIdentifierUri);
        return this;
    }

    public RegisterEndpointBuilder WithScope(IReadOnlyCollection<string> scope)
    {
        _parameters.Add(Parameter.Scope, string.Join(' ', scope));
        return this;
    }

    public RegisterEndpointBuilder WithClientName(string clientName)
    {
        _parameters.Add(Parameter.ClientName, clientName);
        return this;
    }

    public RegisterEndpointBuilder WithGrantTypes(IReadOnlyCollection<string> grantTypes)
    {
        _parameters.Add(Parameter.GrantTypes, grantTypes);
        return this;
    }

    public RegisterEndpointBuilder WithRedirectUris(IReadOnlyCollection<string> redirectUris)
    {
        _parameters.Add(Parameter.RedirectUris, redirectUris);
        return this;
    }

    public RegisterEndpointBuilder WithTokenEndpointAuthMethod(TokenEndpointAuthMethod tokenEndpointAuthMethod)
    {
        _parameters.Add(Parameter.TokenEndpointAuthMethod, tokenEndpointAuthMethod.GetDescription());
        return this;
    }

    public RegisterEndpointBuilder WithJwks(string jwks)
    {
        _parameters.Add(Parameter.Jwks, jwks);
        return this;
    }

    public RegisterEndpointBuilder WithRequestObjectSigningAlg(SigningAlg requestObjectSigningAlg)
    {
        _parameters.Add(Parameter.RequestObjectSigningAlg, requestObjectSigningAlg.GetDescription());
        return this;
    }

    public RegisterEndpointBuilder WithRequireReferenceToken()
    {
        _parameters.Add(Parameter.RequireReferenceToken, true);
        return this;
    }

    internal async Task<RegisterResponse> Post()
    {
        var json = JsonSerializer.Serialize(_parameters);
        var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, "connect/register")
        {
            Content = new StringContent(json, Encoding.UTF8, MimeTypeConstants.Json)
        };
        var httpResponseMessage = await HttpClient.SendAsync(httpRequestMessage);

        TestOutputHelper.WriteLine(
            "Received Register response {0}, Content: {1}",
            httpResponseMessage.StatusCode,
            await httpResponseMessage.Content.ReadAsStringAsync());

        httpResponseMessage.EnsureSuccessStatusCode();
        return (await httpResponseMessage.Content.ReadFromJsonAsync<RegisterResponse>())!;
    }
}
