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
            "GET" => GetRequestFromQuery(httpRequest),
            "POST" => await GetRequestFromBody(httpRequest),
            _ => throw new NotSupportedException("Endpoint only supports GET and POST")
        };
    }

    private static async Task<EndSessionRequest> GetRequestFromBody(HttpRequest httpRequest)
    {
        var formCollection = await httpRequest.ReadFormAsync();
        var idTokenHint = formCollection.GetValue(Parameter.IdTokenHint);
        var clientId = formCollection.GetValue(Parameter.ClientId);
        var postLogoutRedirectUri = formCollection.GetValue(Parameter.PostLogoutRedirectUri);
        var state = formCollection.GetValue(Parameter.State);

        return new EndSessionRequest
        {
            IdTokenHint = idTokenHint,
            ClientId = clientId,
            PostLogoutRedirectUri = postLogoutRedirectUri,
            State = state
        };
    }

    private static EndSessionRequest GetRequestFromQuery(HttpRequest httpRequest)
    {
        var query = httpRequest.Query;
        var idTokenHint = query.GetValue(Parameter.IdTokenHint);
        var clientId = query.GetValue(Parameter.ClientId);
        var postLogoutRedirectUri = query.GetValue(Parameter.PostLogoutRedirectUri);
        var state = query.GetValue(Parameter.State);

        return new EndSessionRequest
        {
            IdTokenHint = idTokenHint,
            ClientId = clientId,
            PostLogoutRedirectUri = postLogoutRedirectUri,
            State = state
        };
    }
}