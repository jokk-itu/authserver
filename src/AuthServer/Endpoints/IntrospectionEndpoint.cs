using AuthServer.Core;
using AuthServer.Core.RequestProcessing;
using AuthServer.Extensions;
using AuthServer.Introspection;
using AuthServer.RequestAccessors.Introspection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AuthServer.Endpoints;
internal static class IntrospectionEndpoint
{
    public static async Task<IResult> HandleIntrospection(
        HttpContext httpContext,
        [FromServices] IRequestAccessor<IntrospectionRequest> requestAccessor,
        [FromServices] IRequestProcessor<IntrospectionRequest, IntrospectionResponse> requestProcessor,
        CancellationToken cancellationToken)
    {
        var request = await requestAccessor.GetRequest(httpContext.Request);
        var result = await requestProcessor.Process(request, cancellationToken);
        return result.Match(
            response => Results.Ok(MapToPostResponse(response)),
            error => error.ResultCode switch
            {
                ResultCode.BadRequest => Results.Extensions.OAuthBadRequest(error),
                _ => Results.Extensions.OAuthInternalServerError(error)
            });
    }

    private static PostIntrospectionResponse MapToPostResponse(IntrospectionResponse response)
    {
        return new PostIntrospectionResponse
        {
            Active = response.Active,
            ClientId = response.ClientId,
            Issuer = response.Issuer,
            Username = response.Username,
            TokenType = response.TokenType,
            Audience = response.Audience,
            ExpiresAt = response.ExpiresAt,
            IssuedAt = response.IssuedAt,
            JwtId = response.JwtId,
            NotBefore = response.NotBefore,
            Scope = response.Scope,
            Subject = response.Subject
        };
    }
}