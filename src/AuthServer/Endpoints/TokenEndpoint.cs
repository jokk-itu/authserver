using AuthServer.Constants;
using AuthServer.Core.Abstractions;
using AuthServer.Core.Request;
using AuthServer.Endpoints.Responses;
using AuthServer.Extensions;
using AuthServer.RequestAccessors.Token;
using AuthServer.TokenByGrant;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace AuthServer.Endpoints;
internal static class TokenEndpoint
{
    public static async Task<IResult> HandleToken(
        HttpContext httpContext,
        [FromServices] IRequestAccessor<TokenRequest> requestAccessor,
        [FromServices] IServiceProvider serviceProvider,
        CancellationToken cancellationToken)
    {
        var request = await requestAccessor.GetRequest(httpContext.Request);

        if (!GrantTypeConstants.GrantTypes.Contains(request.GrantType))
        {
            return Results.Extensions.OAuthBadRequest(TokenError.UnsupportedGrantType);
        }

        var requestHandler = serviceProvider.GetRequiredKeyedService<IRequestHandler<TokenRequest, TokenResponse>>(request.GrantType);
        var result = await requestHandler.Handle(request, cancellationToken);

        return result.Match(
            response => Results.Ok(new PostTokenResponse
            {
                AccessToken = response.AccessToken,
                Scope = response.Scope,
                ExpiresIn = response.ExpiresIn,
                IdToken = response.IdToken,
                RefreshToken = response.RefreshToken
            }),
            error => Results.Extensions.OAuthBadRequest(error));
    }
}