using AuthServer.Authentication.Models;
using AuthServer.Core;
using AuthServer.Core.Abstractions;
using AuthServer.Extensions;
using AuthServer.TokenDecoders;
using Microsoft.AspNetCore.Http;

namespace AuthServer.RequestAccessors.PushedAuthorization;
internal class PushedAuthorizationRequestAccessor : IRequestAccessor<PushedAuthorizationRequest >
{
    public async Task<PushedAuthorizationRequest> GetRequest(HttpRequest httpRequest)
    {
        var body = await httpRequest.ReadFormAsync();

        var loginHint = body.GetValue(Parameter.LoginHint);
        var display = body.GetValue(Parameter.Display);
        var responseMode = body.GetValue(Parameter.ResponseMode);
        var maxAge = body.GetValue(Parameter.MaxAge);
        var codeChallenge = body.GetValue(Parameter.CodeChallenge);
        var codeChallengeMethod = body.GetValue(Parameter.CodeChallengeMethod);
        var redirectUri = body.GetValue(Parameter.RedirectUri);
        var idTokenHint = body.GetValue(Parameter.IdTokenHint);
        var prompt = body.GetValue(Parameter.Prompt);
        var responseType = body.GetValue(Parameter.ResponseType);
        var nonce = body.GetValue(Parameter.Nonce);
        var state = body.GetValue(Parameter.State);
        var requestObject = body.GetValue(Parameter.Request);

        var scope = body.GetSpaceDelimitedValue(Parameter.Scope);
        var acrValues = body.GetSpaceDelimitedValue(Parameter.AcrValues);

        var clientSecretBasic = httpRequest.GetClientSecretBasic();
        var clientSecretPost = body.GetClientSecretPost();
        var clientAssertion = body.GetClientAssertion(ClientTokenAudience.PushedAuthorizeEndpoint);

        var clientAuthentications = new List<ClientAuthentication>();
        if (clientSecretBasic is not null) clientAuthentications.Add(clientSecretBasic);
        if (clientSecretPost is not null) clientAuthentications.Add(clientSecretPost);
        if (clientAssertion is not null) clientAuthentications.Add(clientAssertion);

        return new PushedAuthorizationRequest
        {
            IdTokenHint = idTokenHint,
            LoginHint = loginHint,
            Prompt = prompt,
            Display = display,
            RedirectUri = redirectUri,
            CodeChallenge = codeChallenge,
            CodeChallengeMethod = codeChallengeMethod,
            ResponseType = responseType,
            Nonce = nonce,
            MaxAge = maxAge,
            State = state,
            ResponseMode = responseMode,
            RequestObject = requestObject,
            Scope = scope,
            AcrValues = acrValues,
            ClientAuthentications = clientAuthentications.AsReadOnly()
        };
    }
}
