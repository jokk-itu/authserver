using WebApp.Constants;
using WebApp.Context.Abstract;

namespace WebApp.Context.AuthorizeContext;

public class AuthorizeContextAccessor : IContextAccessor<AuthorizeContext>
{
    public Task<AuthorizeContext> GetContext(HttpContext httpContext)
    {
        var query = httpContext.Request.Query;
        var context = new AuthorizeContext();
        if (query.TryGetValue(ParameterNames.MaxAge, out var maxAge))
        {
            context.MaxAge = maxAge;
        }
        if (query.TryGetValue(ParameterNames.ClientId, out var clientId))
        {
            context.ClientId = clientId;
        }
        if (query.TryGetValue(ParameterNames.CodeChallenge, out var codeChallenge))
        {
            context.CodeChallenge = codeChallenge;
        }
        if (query.TryGetValue(ParameterNames.CodeChallengeMethod, out var codeChallengeMethod))
        {
            context.CodeChallengeMethod = codeChallengeMethod;
        }
        if (query.TryGetValue(ParameterNames.RedirectUri, out var redirectUri))
        {
            context.RedirectUri = redirectUri;
        }
        if (query.TryGetValue(ParameterNames.IdTokenHint, out var idTokenHint))
        {
            context.IdTokenHint = idTokenHint;
        }
        if (query.TryGetValue(ParameterNames.Scope, out var scope))
        {
            context.Scope = scope;
        }
        if (query.TryGetValue(ParameterNames.Prompt, out var prompt))
        {
            context.Prompt = prompt;
        }
        if (query.TryGetValue(ParameterNames.ResponseType, out var responseType))
        {
            context.ResponseType = responseType;
        }
        if (query.TryGetValue(ParameterNames.Nonce, out var nonce))
        {
            context.Nonce = nonce;
        }
        if (query.TryGetValue(ParameterNames.State, out var state))
        {
            context.State = state;
        }

        return Task.FromResult(context);
    }
}
