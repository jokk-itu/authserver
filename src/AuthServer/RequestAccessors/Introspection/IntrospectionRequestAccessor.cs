using AuthServer.Core;
using AuthServer.Core.Models;
using Microsoft.AspNetCore.Http;
using AuthServer.Extensions;
using AuthServer.TokenDecoders;

namespace AuthServer.RequestAccessors.Introspection;
internal class RevocationRequestAccessor : IRequestAccessor<IntrospectionRequest>
{
    public async Task<IntrospectionRequest> GetRequest(HttpRequest httpRequest)
    {
        var body = await httpRequest.ReadFormAsync();
        var token = body.GetValueOrEmpty(Parameter.Token);
        var tokenTypeHint = body.GetValueOrEmpty(Parameter.TokenTypeHint);
        var clientSecretBasic = httpRequest.GetClientSecretBasic();
        var clientSecretPost = body.GetClientSecretPost();
        var clientAssertion = body.GetClientAssertion(ClientTokenAudience.IntrospectionEndpoint);

        var clientAuthentications = new List<ClientAuthentication>();
        if (clientSecretBasic is not null) clientAuthentications.Add(clientSecretBasic);
        if (clientSecretPost is not null) clientAuthentications.Add(clientSecretPost);
        if (clientAssertion is not null) clientAuthentications.Add(clientAssertion);

        return new IntrospectionRequest
        {
            Token = token,
            TokenTypeHint = tokenTypeHint,
            ClientAuthentications = clientAuthentications.AsReadOnly()
        };
    }
}