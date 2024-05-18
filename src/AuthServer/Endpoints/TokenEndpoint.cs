﻿using AuthServer.Constants;
using AuthServer.Core;
using AuthServer.Core.RequestProcessing;
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

        var requestProcessor = serviceProvider.GetRequiredKeyedService<IRequestProcessor<TokenRequest, TokenResponse>>(request.GrantType);
        var result = await requestProcessor.Process(request, cancellationToken);

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