using Infrastructure.Requests.CreateAuthorizationGrant;
using WebApp.Constants;

namespace WebApp.Extensions;

public static class QueryCollectionExtensions
{
  public static RouteValueDictionary ToRouteValueDictionary(this IQueryCollection query)
  {
    var routeValues = new RouteValueDictionary();
    foreach (var (key, value) in query)
    {
      routeValues.Add(key, value);
    }

    return routeValues;
  }

  public static CreateAuthorizationGrantCommand ToAuthorizationGrantCommand(this IQueryCollection query, string userId)
  {
    var isMaxAgeParsed = long.TryParse(query[ParameterNames.MaxAge], out var maxAge);
    return new CreateAuthorizationGrantCommand
    {
      UserId = userId,
      ClientId = query[ParameterNames.ClientId],
      CodeChallenge = query[ParameterNames.CodeChallenge],
      CodeChallengeMethod = query[ParameterNames.CodeChallengeMethod],
      ResponseType = query[ParameterNames.ResponseType],
      Nonce = query[ParameterNames.Nonce],
      RedirectUri = query[ParameterNames.RedirectUri],
      Scopes = query[ParameterNames.Scope].ToString().Split(' '),
      MaxAge = isMaxAgeParsed ? maxAge : 0L,
      State = query[ParameterNames.State]
    };
  }
}
