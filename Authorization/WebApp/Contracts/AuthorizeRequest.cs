using Microsoft.AspNetCore.Mvc;
using WebApp.Constants;

namespace WebApp.Contracts;

#nullable disable
public class AuthorizeRequest
{
  [FromQuery(Name = ParameterNames.Scope)]
  public string Scope { get; init; }

  [FromQuery(Name = ParameterNames.IdTokenHint)]
  public string IdTokenHint { get; init; }

  [FromQuery(Name = ParameterNames.Prompt)]
  public string Prompt { get; init; }

  [FromQuery(Name = ParameterNames.ClientId)]
  public string ClientId { get; init; }

  [FromQuery(Name = ParameterNames.RedirectUri)]
  public string RedirectUri { get; init; }

  [FromQuery(Name = ParameterNames.CodeChallenge)]
  public string CodeChallenge { get; init; }

  [FromQuery(Name = ParameterNames.CodeChallengeMethod)]
  public string CodeChallengeMethod { get; init; }

  [FromQuery(Name = ParameterNames.ResponseType)]
  public string ResponseType { get; init; }

  [FromQuery(Name = ParameterNames.Nonce)]
  public string Nonce { get; init; }

  [FromQuery(Name = ParameterNames.MaxAge)]
  public string MaxAge { get; init; }

  [FromQuery(Name = ParameterNames.State)]
  public string State { get; init; }
}