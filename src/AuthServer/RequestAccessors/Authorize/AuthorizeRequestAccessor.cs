using AuthServer.Core;
using AuthServer.Extensions;
using Microsoft.AspNetCore.Http;

namespace AuthServer.RequestAccessors.Authorize;
internal class AuthorizeRequestAccessor : IRequestAccessor<AuthorizeRequest>
{
    public Task<AuthorizeRequest> GetRequest(HttpRequest httpRequest)
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
        var scope = query.GetSpaceDelimitedValueOrEmpty(Parameter.Scope);
        var acrValues = query.GetSpaceDelimitedValueOrEmpty(Parameter.AcrValues);

        return Task.FromResult(new AuthorizeRequest
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
            Scope = scope,
            AcrValues = acrValues
        });
    }
}