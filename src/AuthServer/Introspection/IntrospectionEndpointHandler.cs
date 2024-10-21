using AuthServer.Core.Abstractions;
using AuthServer.Core.Request;
using AuthServer.Endpoints.Responses;
using AuthServer.Extensions;
using AuthServer.RequestAccessors.Introspection;
using Microsoft.AspNetCore.Http;

namespace AuthServer.Introspection;
internal class IntrospectionEndpointHandler : IEndpointHandler
{
    private readonly IRequestAccessor<IntrospectionRequest> _requestAccessor;
    private readonly IRequestHandler<IntrospectionRequest, IntrospectionResponse> _requestHandler;

    public IntrospectionEndpointHandler(
        IRequestAccessor<IntrospectionRequest> requestAccessor,
        IRequestHandler<IntrospectionRequest, IntrospectionResponse> requestHandler)
    {
        _requestAccessor = requestAccessor;
        _requestHandler = requestHandler;
    }

    public async Task<IResult> Handle(HttpContext httpContext, CancellationToken cancellationToken)
    {
        var request = await _requestAccessor.GetRequest(httpContext.Request);
        var result = await _requestHandler.Handle(request, cancellationToken);
        return result.Match(
            response => Results.Ok(new PostIntrospectionResponse
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
            }),
            error => Results.Extensions.OAuthBadRequest(error));
    }
}
