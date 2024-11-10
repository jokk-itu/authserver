using AuthServer.Authentication.Models;
using AuthServer.Core;
using Microsoft.AspNetCore.Http;
using AuthServer.Extensions;
using AuthServer.TokenDecoders;
using AuthServer.Core.Abstractions;

namespace AuthServer.RequestAccessors.Revocation;
internal class RevocationRequestAccessor : IRequestAccessor<RevocationRequest>
{
    public async Task<RevocationRequest> GetRequest(HttpRequest httpRequest)
    {
        var body = await httpRequest.ReadFormAsync();
        var token = body.GetValue(Parameter.Token);
        var tokenTypeHint = body.GetValue(Parameter.TokenTypeHint);
        var clientSecretBasic = httpRequest.GetClientSecretBasic();
        var clientSecretPost = body.GetClientSecretPost();
        var clientAssertion = body.GetClientAssertion(ClientTokenAudience.RevocationEndpoint);

        var clientAuthentications = new List<ClientAuthentication>();
        if (clientSecretBasic is not null) clientAuthentications.Add(clientSecretBasic);
        if (clientSecretPost is not null) clientAuthentications.Add(clientSecretPost);
        if (clientAssertion is not null) clientAuthentications.Add(clientAssertion);

        return new RevocationRequest
        {
            Token = token,
            TokenTypeHint = tokenTypeHint,
            ClientAuthentications = clientAuthentications.AsReadOnly()
        };
    }
}