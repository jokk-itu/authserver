using System.Text;
using System.Text.Encodings.Web;
using AuthServer.Authorize.Abstractions;
using AuthServer.Cache.Abstractions;
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
    private readonly IAuthorizeRequestParameterProcessor _authorizeRequestParameterProcessor;

    public AuthorizeResponseBuilder(
        IOptionsSnapshot<DiscoveryDocument> discoveryDocumentOptions,
        ICachedClientStore cachedClientStore,
        IAuthorizeRequestParameterProcessor authorizeRequestParameterProcessor)
    {
        _discoveryDocumentOptions = discoveryDocumentOptions;
        _cachedClientStore = cachedClientStore;
        _authorizeRequestParameterProcessor = authorizeRequestParameterProcessor;
    }

    /// <inheritdoc />
    public async Task<IResult> BuildResponse(AuthorizeRequest request, IDictionary<string, string> additionalParameters, HttpResponse httpResponse, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrEmpty(request.RequestUri)
            || !string.IsNullOrEmpty(request.RequestObject))
        {
            request = _authorizeRequestParameterProcessor.GetCachedRequest();
        }

        var responseMode = request.ResponseMode;
        if (string.IsNullOrEmpty(responseMode))
        {
            responseMode = DeduceResponseMode(request.ResponseType);
        }

        additionalParameters.Add(Parameter.State, request.State);

        if (responseMode is ResponseModeConstants.FormPost)
        {
            additionalParameters.Add(Parameter.Issuer, _discoveryDocumentOptions.Value.Issuer);
        }

        var redirectUri = request.RedirectUri;
        if (string.IsNullOrEmpty(redirectUri))
        {
            var cachedClient = await _cachedClientStore.Get(request.ClientId, cancellationToken);
            redirectUri = cachedClient.RedirectUris.Single();
        }

        return responseMode switch
        {
            ResponseModeConstants.Query => BuildQuery(redirectUri, additionalParameters, httpResponse),
            ResponseModeConstants.Fragment => BuildFragment(redirectUri, additionalParameters, httpResponse),
            ResponseModeConstants.FormPost => BuildFormPost(redirectUri, additionalParameters),
            _ => throw new ArgumentException("Unexpected response_mode value", nameof(request))
        };
    }

    private static IResult BuildQuery(string redirectUri, IDictionary<string, string> additionalParameters, HttpResponse httpResponse)
    {
        var builder = new StringBuilder();
        builder.Append('?');
        AddParameters(builder, additionalParameters);
        var query = builder.ToString();
        return Results.Extensions.OAuthSeeOtherRedirect(redirectUri + query, httpResponse);
    }

    private static IResult BuildFragment(string redirectUri, IDictionary<string, string> additionalParameters, HttpResponse httpResponse)
    {
        var builder = new StringBuilder();
        builder.Append('#');
        AddParameters(builder, additionalParameters);
        var fragment = builder.ToString();
        return Results.Extensions.OAuthSeeOtherRedirect(redirectUri + fragment, httpResponse);
    }

    private static void AddParameters(StringBuilder builder, IDictionary<string, string> parameters)
    {
        builder.AppendJoin('&', parameters.Select(x => x.Key + '=' + x.Value));
    }

    private static IResult BuildFormPost(string redirectUri, IDictionary<string, string> additionalParameters)
    {
        var formPrefix = $"""<html><head><title>Submit Form</title></head><body onload="javascript:document.forms[0].submit()"><form method="post" action="{redirectUri}">""";
        const string formSuffix = "</form></body></html>";
        var formBuilder = new StringBuilder();

        formBuilder.Append(formPrefix);
        foreach (var parameter in additionalParameters)
        {
            formBuilder.Append($"""<input type="hidden" name="{parameter.Key}" value="{parameter.Value}" />""");
        }
        formBuilder.Append(formSuffix);

        return Results.Extensions.OAuthOkWithHtml(formBuilder.ToString());
    }

    private static string DeduceResponseMode(string responseType)
    {
        return responseType switch
        {
            ResponseTypeConstants.Code => ResponseModeConstants.Query,
            _ => throw new ArgumentException("Unexpected value", nameof(responseType))
        };
    }
}