using AuthServer.Core;
using AuthServer.Core.Abstractions;
using AuthServer.Core.Models;
using AuthServer.Extensions;
using AuthServer.TokenDecoders;
using Microsoft.AspNetCore.Http;

namespace AuthServer.RequestAccessors.PushedAuthorization;
internal class PushedAuthorizationRequestAccessor : IRequestAccessor<PushedAuthorizationRequest >
{
    public async Task<PushedAuthorizationRequest> GetRequest(HttpRequest httpRequest)
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

        var scope = body.GetSpaceDelimitedValueOrEmpty(Parameter.Scope);
        var acrValues = body.GetSpaceDelimitedValueOrEmpty(Parameter.AcrValues);

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
            Scope = scope,
            AcrValues = acrValues,
            ClientAuthentications = clientAuthentications.AsReadOnly()
        };
    }
}
