using AuthServer.Core;
using AuthServer.Core.Abstractions;
using AuthServer.Extensions;
using Microsoft.AspNetCore.Http;

namespace AuthServer.RequestAccessors.Authorize;
internal class AuthorizeRequestAccessor : IRequestAccessor<AuthorizeRequest>
{
    public async Task<AuthorizeRequest> GetRequest(HttpRequest httpRequest)
    {
        return httpRequest.Method switch
        {
            "GET" => GetRequestFromQuery(httpRequest),
            "POST" => await GetRequestFromBody(httpRequest),
            _ => throw new NotSupportedException("Endpoint only supports GET and POST")
        };
    }

    private static AuthorizeRequest GetRequestFromQuery(HttpRequest httpRequest)
    {
        var query = httpRequest.Query;

        var loginHint = query.GetValueOrEmpty(Parameter.LoginHint);
        var display = query.GetValueOrEmpty(Parameter.Display);
        var responseMode = query.GetValueOrEmpty(Parameter.ResponseMode);
        var maxAge = query.GetValueOrEmpty(Parameter.MaxAge);
        var clientId = query.GetValueOrEmpty(Parameter.ClientId);
        var codeChallenge = query.GetValueOrEmpty(Parameter.CodeChallenge);
        var codeChallengeMethod = query.GetValueOrEmpty(Parameter.CodeChallengeMethod);
        var redirectUri = query.GetValueOrEmpty(Parameter.RedirectUri);
        var idTokenHint = query.GetValueOrEmpty(Parameter.IdTokenHint);
        var prompt = query.GetValueOrEmpty(Parameter.Prompt);
        var responseType = query.GetValueOrEmpty(Parameter.ResponseType);
        var nonce = query.GetValueOrEmpty(Parameter.Nonce);
        var state = query.GetValueOrEmpty(Parameter.State);
        var requestObject = query.GetValueOrEmpty(Parameter.Request);
        var requestUri = query.GetValueOrEmpty(Parameter.RequestUri);

        var scope = query.GetSpaceDelimitedValueOrEmpty(Parameter.Scope);
        var acrValues = query.GetSpaceDelimitedValueOrEmpty(Parameter.AcrValues);

        return new AuthorizeRequest
        {
            IdTokenHint = idTokenHint,
            LoginHint = loginHint,
            Prompt = prompt,
            Display = display,
            ClientId = clientId,
            RedirectUri = redirectUri,
            CodeChallenge = codeChallenge,
            CodeChallengeMethod = codeChallengeMethod,
            ResponseType = responseType,
            Nonce = nonce,
            MaxAge = maxAge,
            State = state,
            ResponseMode = responseMode,
            RequestObject = requestObject,
            RequestUri = requestUri,
            Scope = scope,
            AcrValues = acrValues
        };
    }

    private static async Task<AuthorizeRequest> GetRequestFromBody(HttpRequest httpRequest)
    {
        var body = await httpRequest.ReadFormAsync();

        var loginHint = body.GetValueOrEmpty(Parameter.LoginHint);
        var display = body.GetValueOrEmpty(Parameter.Display);
        var responseMode = body.GetValueOrEmpty(Parameter.ResponseMode);
        var maxAge = body.GetValueOrEmpty(Parameter.MaxAge);
        var clientId = body.GetValueOrEmpty(Parameter.ClientId);
        var codeChallenge = body.GetValueOrEmpty(Parameter.CodeChallenge);
        var codeChallengeMethod = body.GetValueOrEmpty(Parameter.CodeChallengeMethod);
        var redirectUri = body.GetValueOrEmpty(Parameter.RedirectUri);
        var idTokenHint = body.GetValueOrEmpty(Parameter.IdTokenHint);
        var prompt = body.GetValueOrEmpty(Parameter.Prompt);
        var responseType = body.GetValueOrEmpty(Parameter.ResponseType);
        var nonce = body.GetValueOrEmpty(Parameter.Nonce);
        var state = body.GetValueOrEmpty(Parameter.State);
        var requestObject = body.GetValueOrEmpty(Parameter.Request);
        var requestUri = body.GetValueOrEmpty(Parameter.RequestUri);

        var scope = body.GetSpaceDelimitedValueOrEmpty(Parameter.Scope);
        var acrValues = body.GetSpaceDelimitedValueOrEmpty(Parameter.AcrValues);

        return new AuthorizeRequest
        {
            IdTokenHint = idTokenHint,
            LoginHint = loginHint,
            Prompt = prompt,
            Display = display,
            ClientId = clientId,
            RedirectUri = redirectUri,
            CodeChallenge = codeChallenge,
            CodeChallengeMethod = codeChallengeMethod,
            ResponseType = responseType,
            Nonce = nonce,
            MaxAge = maxAge,
            State = state,
            ResponseMode = responseMode,
            RequestObject = requestObject,
            RequestUri = requestUri,
            Scope = scope,
            AcrValues = acrValues
        };
    }
}