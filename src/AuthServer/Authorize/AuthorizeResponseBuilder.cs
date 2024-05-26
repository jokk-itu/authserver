using System.Text;
using AuthServer.Authorize.Abstract;
using AuthServer.Cache;
using AuthServer.Constants;
using AuthServer.Core.Discovery;
using AuthServer.Extensions;
using AuthServer.RequestAccessors.Authorize;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Parameter = AuthServer.Core.Parameter;

namespace AuthServer.Authorize;

internal class AuthorizeResponseBuilder : IAuthorizeResponseBuilder
{
    private readonly IOptionsSnapshot<DiscoveryDocument> _discoveryDocumentOptions;
    private readonly ICachedClientStore _cachedClientStore;

    public AuthorizeResponseBuilder(
        IOptionsSnapshot<DiscoveryDocument> discoveryDocumentOptions,
        ICachedClientStore cachedClientStore)
    {
        _discoveryDocumentOptions = discoveryDocumentOptions;
        _cachedClientStore = cachedClientStore;
    }

    /// <inheritdoc />
    public async Task<IResult> BuildResponse(AuthorizeRequest request, IDictionary<string, string> additionalParameters, CancellationToken cancellationToken)
    {
        var responseMode = request.ResponseMode;
        if (string.IsNullOrEmpty(responseMode))
        {
            responseMode = DeduceResponseMode(request.ResponseType);
        }

        responseMode = responseMode == ResponseModeConstants.Jwt
            ? SubstituteResponseMode(request.ResponseType)
            : responseMode;

        additionalParameters.Add(Parameter.State, request.State);

        var redirectUri = request.RedirectUri;
        if (string.IsNullOrEmpty(redirectUri))
        {
            var cachedClient = await _cachedClientStore.Get(request.ClientId, cancellationToken);
            redirectUri = cachedClient.RedirectUris.Single();
        }

        return responseMode switch
        {
            ResponseModeConstants.FormPost => BuildFormPost(redirectUri, additionalParameters),
            ResponseModeConstants.FormPostJwt => throw new NotImplementedException(),
            _ => throw new ArgumentException("Unexpected response_mode value", nameof(request))
        };
    }

    private IResult BuildFormPost(string redirectUri, IDictionary<string, string> additionalParameters)
    {
        var formPrefix = $"""<html><head><title>Submit Form</title></head><body onload="javascript:document.forms[0].submit()"><form method="post" action="{redirectUri}">""";
        var formSuffix = "</form></body></html>";
        var formBuilder = new StringBuilder();

        formBuilder.Append(formPrefix);
        foreach (var parameter in additionalParameters)
        {
            formBuilder.Append($"""<input type="hidden" name="{parameter.Key}" value="{parameter.Value}" />""");
        }
        formBuilder.Append($"""<input type="hidden" name="{Parameter.Issuer}" value="{_discoveryDocumentOptions.Value.Issuer}" />""");
        formBuilder.Append(formSuffix);

        return Results.Extensions.OAuthOkWithHtml(formBuilder.ToString());
    }

    private static string DeduceResponseMode(string responseType)
    {
        return responseType switch
        {
            ResponseTypeConstants.Code => ResponseModeConstants.FormPost,
            _ => throw new ArgumentException("Unexpected value", nameof(responseType))
        };
    }

    private static string SubstituteResponseMode(string responseType)
    {
        return responseType switch
        {
            ResponseTypeConstants.Code => ResponseModeConstants.FormPostJwt,
            _ => throw new ArgumentException("Unexpected value", nameof(responseType))
        };
    }
}