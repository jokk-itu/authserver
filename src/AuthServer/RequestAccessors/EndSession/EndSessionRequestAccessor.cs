using AuthServer.Core;
using AuthServer.Core.Abstractions;
using AuthServer.Extensions;
using Microsoft.AspNetCore.Http;

namespace AuthServer.RequestAccessors.EndSession;

internal class EndSessionRequestAccessor : IRequestAccessor<EndSessionRequest>
{
    public async Task<EndSessionRequest> GetRequest(HttpRequest httpRequest)
    {
        return httpRequest.Method switch
        {
            "GET" => GetContextFromQuery(httpRequest),
            "POST" => await GetContextFromBody(httpRequest),
            _ => throw new NotSupportedException("Endpoint only supports GET and POST")
        };
    }

    private static async Task<EndSessionRequest> GetContextFromBody(HttpRequest httpRequest)
    {
        var formCollection = await httpRequest.ReadFormAsync();
        var idTokenHint = formCollection.GetValueOrEmpty(Parameter.IdTokenHint);
        var clientId = formCollection.GetValueOrEmpty(Parameter.ClientId);
        var postLogoutRedirectUri = formCollection.GetValueOrEmpty(Parameter.PostLogoutRedirectUri);
        var state = formCollection.GetValueOrEmpty(Parameter.State);

        return new EndSessionRequest
        {
            IdTokenHint = idTokenHint,
            ClientId = clientId,
            PostLogoutRedirectUri = postLogoutRedirectUri,
            State = state
        };
    }

    private static EndSessionRequest GetContextFromQuery(HttpRequest httpRequest)
    {
        var query = httpRequest.Query;
        var idTokenHint = query.GetValueOrEmpty(Parameter.IdTokenHint);
        var clientId = query.GetValueOrEmpty(Parameter.ClientId);
        var postLogoutRedirectUri = query.GetValueOrEmpty(Parameter.PostLogoutRedirectUri);
        var state = query.GetValueOrEmpty(Parameter.State);

        return new EndSessionRequest
        {
            IdTokenHint = idTokenHint,
            ClientId = clientId,
            PostLogoutRedirectUri = postLogoutRedirectUri,
            State = state
        };
    }
}