using Contracts.UserLogin;
using Microsoft.AspNetCore.Mvc;

namespace Contracts.AuthorizeCode;

public record PostAuthorizeCodeRequest
{
  [FromBody]
  public PostUserLoginRequest UserInformation { get; init; }

  [FromQuery(Name = "response_type")]
  public string ResponseType { get; init; }

  [FromQuery(Name = "client_id")]
  public string ClientId { get; init; }

  [FromQuery(Name = "redirect_uri")]
  public string RedirectUri { get; init; }

  [FromQuery(Name = "scope")]
  public string Scope { get; init; }

  [FromQuery(Name = "state")]
  public string State { get; init; }

  [FromQuery(Name = "code_challenge")]
  public string CodeChallenge { get; init; }

  [FromQuery(Name = "code_challenge_method")]
  public string CodeChallengeMethod => "plain";

  [FromQuery(Name = "nonce")]
  public string Nonce { get; init; }

  [FromQuery(Name = "display")]
  public string Display => "page";
}