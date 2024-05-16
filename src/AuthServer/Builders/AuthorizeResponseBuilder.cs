using AuthServer.Constants;
using AuthServer.Core;
using Microsoft.AspNetCore.Mvc;

namespace AuthServer.Builders;
public class AuthorizeResponseBuilder : IAuthorizeResponseBuilder
{
    // response_mode parameter nullable
    // response_type parameter required
    // state parameter required
    // iss (injectes internt)

    // Scenario specific variables
    // code
    // error
    // error_description

    // JWT build function
    // Redirect uri query function
    // Redirect uri fragment function
    // OK form_post function
    public IActionResult BuildResponse(string redirectUri, string state, string responseType, string? responseMode,
        IDictionary<string, string> additionalParameters)
    {
        responseMode ??= DeduceResponseMode(responseType);
        responseMode = responseMode == ResponseModeConstants.Jwt ? SubstituteResponseMode(responseType) : responseMode;

        additionalParameters.Add(Parameter.State, state);

        return responseMode switch
        {
            ResponseModeConstants.Query => BuildQuery(),
            ResponseModeConstants.Fragment => BuildFragment(),
            ResponseModeConstants.FormPost => BuildFormPost(),
            ResponseModeConstants.QueryJwt => throw new NotImplementedException(),
            ResponseModeConstants.FragmentJwt => throw new NotImplementedException(),
            ResponseModeConstants.FormPostJwt => throw new NotImplementedException(),
            _ => throw new ArgumentException("Unexpected value", nameof(responseMode))
        };
    }

    private static IActionResult BuildQuery()
    {
        throw new NotImplementedException();
    }

    private static IActionResult BuildFragment()
    {
        throw new NotImplementedException();
    }

    private static IActionResult BuildFormPost()
    {
        throw new NotImplementedException();
    }

    private static string DeduceResponseMode(string responseType)
    {
        return responseType switch
        {
            ResponseTypeConstants.Code => ResponseModeConstants.Query,
            _ => throw new ArgumentException("Unexpected value", nameof(responseType))
        };
    }

    private static string SubstituteResponseMode(string responseType)
    {
        return responseType switch
        {
            ResponseTypeConstants.Code => ResponseModeConstants.QueryJwt,
            _ => throw new ArgumentException("Unexpected value", nameof(responseType))
        };
    }
}