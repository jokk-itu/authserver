using AuthServer.Constants;
using AuthServer.Core.Abstractions;
using AuthServer.Core.Request;
using AuthServer.Endpoints.Responses;
using AuthServer.Extensions;
using AuthServer.RequestAccessors.Token;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace AuthServer.TokenByGrant;
internal class TokenEndpointHandler : IEndpointHandler
{
    private readonly IRequestAccessor<TokenRequest> _requestAccessor;
    private readonly IServiceProvider _serviceProvider;

    public TokenEndpointHandler(
        IRequestAccessor<TokenRequest> requestAccessor,
        IServiceProvider serviceProvider)
    {
        _requestAccessor = requestAccessor;
        _serviceProvider = serviceProvider;
    }

    public async Task<IResult> Handle(HttpContext httpContext, CancellationToken cancellationToken)
    {
        var request = await _requestAccessor.GetRequest(httpContext.Request);

        if (!GrantTypeConstants.GrantTypes.Contains(request.GrantType))
        {
            return Results.Extensions.OAuthBadRequest(TokenError.UnsupportedGrantType);
        }

        var requestHandler = _serviceProvider.GetRequiredKeyedService<IRequestHandler<TokenRequest, TokenResponse>>(request.GrantType);
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
