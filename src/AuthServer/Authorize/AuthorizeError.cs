using AuthServer.Core;
using AuthServer.Core.Request;

namespace AuthServer.Authorize;

internal class AuthorizeError
{
    public static readonly ProcessError InvalidState =
        new(ErrorCode.InvalidRequest, "state must not be null or empty", ResultCode.BadRequest);

    public static readonly ProcessError InvalidClient =
        new(ErrorCode.InvalidClient, "client_id is invalid", ResultCode.BadRequest);

    public static readonly ProcessError InvalidRedirectUri =
        new(ErrorCode.InvalidRequest, "redirect_uri must not be null or empty", ResultCode.BadRequest);

    public static readonly ProcessError UnauthorizedRedirectUri =
        new(ErrorCode.UnauthorizedClient, "client is unauthorized for redirect_uri", ResultCode.BadRequest);

    public static readonly ProcessError InvalidResponseMode =
        new(ErrorCode.InvalidRequest, "response_mode is invalid", ResultCode.Redirect);

    public static readonly ProcessError InvalidResponseType =
        new(ErrorCode.InvalidRequest, "response_type is invalid", ResultCode.Redirect);

    public static readonly ProcessError UnauthorizedResponseType =
        new(ErrorCode.UnauthorizedClient, "client is unauthorized for authorization_code", ResultCode.Redirect);

    public static readonly ProcessError InvalidDisplay =
        new(ErrorCode.InvalidRequest, "display is invalid", ResultCode.Redirect);

    public static readonly ProcessError InvalidNonce =
        new(ErrorCode.InvalidRequest, "nonce must not be null or empty", ResultCode.Redirect);

    public static readonly ProcessError ReplayNonce =
        new(ErrorCode.InvalidRequest, "nonce replay attack detected", ResultCode.Redirect);

    public static readonly ProcessError InvalidCodeChallengeMethod =
        new(ErrorCode.InvalidRequest, "code_challenge_method is invalid", ResultCode.Redirect);

    public static readonly ProcessError InvalidCodeChallenge =
        new(ErrorCode.InvalidRequest, "code_challenge is invalid", ResultCode.Redirect);

    public static readonly ProcessError InvalidOpenIdScope =
        new(ErrorCode.InvalidScope, "openid is required", ResultCode.Redirect);

    public static readonly ProcessError UnauthorizedScope =
        new(ErrorCode.UnauthorizedClient, "client is unauthorized for scope", ResultCode.Redirect);

    public static readonly ProcessError InvalidMaxAge =
        new(ErrorCode.InvalidRequest, "max_age is invalid", ResultCode.Redirect);

    public static readonly ProcessError InvalidIdTokenHint =
        new(ErrorCode.InvalidRequest, "id_token_hint is invalid", ResultCode.Redirect);

    public static readonly ProcessError InvalidPrompt =
        new(ErrorCode.InvalidRequest, "prompt is invalid", ResultCode.Redirect);

    public static readonly ProcessError InvalidAcr =
        new(ErrorCode.InvalidRequest, "acr_values is invalid", ResultCode.Redirect);

    public static readonly ProcessError LoginRequired =
        new(ErrorCode.LoginRequired, "login is required", ResultCode.Redirect);

    public static readonly ProcessError ConsentRequired =
        new(ErrorCode.ConsentRequired, "consent is required", ResultCode.Redirect);

    public static readonly ProcessError AccountSelectionRequired =
        new(ErrorCode.AccountSelectionRequired, "select_account is required", ResultCode.Redirect);
}