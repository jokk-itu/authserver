using AuthServer.Core;
using AuthServer.Core.Models;
using Microsoft.AspNetCore.Http;
using AuthServer.Extensions;
using AuthServer.TokenDecoders;
using AuthServer.Core.Abstractions;

namespace AuthServer.RequestAccessors.Token;

internal class TokenRequestAccessor : IRequestAccessor<TokenRequest>
{
    public async Task<TokenRequest> GetRequest(HttpRequest httpRequest)
    {
        var body = await httpRequest.ReadFormAsync();
        var grantType = body.GetValueOrEmpty(Parameter.GrantType);
        var code = body.GetValueOrEmpty(Parameter.Code);
        var codeVerifier = body.GetValueOrEmpty(Parameter.CodeVerifier);
        var redirectUri = body.GetValueOrEmpty(Parameter.RedirectUri);
        var refreshToken = body.GetValueOrEmpty(Parameter.RefreshToken);
        var dPoPToken = body.GetValueOrEmpty(Parameter.DPoP);

        var scope = body.GetSpaceDelimitedValueOrEmpty(Parameter.Scope);
        var resource = body.GetSpaceDelimitedValueOrEmpty(Parameter.Resource);

        var clientSecretBasic = httpRequest.GetClientSecretBasic();
        var clientSecretPost = body.GetClientSecretPost();
        var clientAssertion = body.GetClientAssertion(ClientTokenAudience.TokenEndpoint);
        var clientId = body.GetClientId();

        var clientAuthentications = new List<ClientAuthentication>();
        if (clientSecretBasic is not null) clientAuthentications.Add(clientSecretBasic);
        if (clientSecretPost is not null) clientAuthentications.Add(clientSecretPost);
        if (clientAssertion is not null) clientAuthentications.Add(clientAssertion);
        if (clientId is not null) clientAuthentications.Add(clientId);

        return new TokenRequest
        {
            GrantType = grantType,
            Code = code,
            CodeVerifier = codeVerifier,
            RedirectUri = redirectUri,
            RefreshToken = refreshToken,
            DPoPToken = dPoPToken,
            Scope = scope,
            Resource = resource,
            ClientAuthentications = clientAuthentications
        };
    }
}