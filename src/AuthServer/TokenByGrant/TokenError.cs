using AuthServer.Core;
using AuthServer.Core.Request;

namespace AuthServer.TokenByGrant;

internal static class TokenError
{
    public static readonly ProcessError UnsupportedGrantType =
        new(ErrorCode.UnsupportedGrantType, "grant_type is unsupported", ResultCode.BadRequest);

    public static readonly ProcessError InvalidCodeVerifier =
        new(ErrorCode.InvalidRequest, "code_verifier is invalid", ResultCode.BadRequest);

    public static readonly ProcessError MultipleOrNoneClientMethod = 
        new(ErrorCode.InvalidClient, "multiple or none client authentication methods detected", ResultCode.BadRequest);

    public static readonly ProcessError InvalidClient =
        new(ErrorCode.InvalidClient, "client could not be authenticated", ResultCode.BadRequest);

    public static readonly ProcessError UnauthorizedForGrantType =
        new(ErrorCode.UnauthorizedClient, "client is unauthorized for grant_type", ResultCode.BadRequest);

    public static readonly ProcessError UnauthorizedForScope =
        new(ErrorCode.UnauthorizedClient, "client is unauthorized for scope", ResultCode.BadRequest);

    public static readonly ProcessError InvalidTarget =
        new(ErrorCode.InvalidTarget, "resource is invalid", ResultCode.BadRequest);

    public static readonly ProcessError InvalidCode =
        new(ErrorCode.InvalidRequest, "code is invalid", ResultCode.BadRequest);

    public static readonly ProcessError InvalidRefreshToken =
        new(ErrorCode.InvalidRequest, "refresh_token is invalid", ResultCode.BadRequest);

    public static readonly ProcessError InvalidRedirectUri =
        new(ErrorCode.InvalidRequest, "redirect_uri is invalid", ResultCode.BadRequest);

    public static readonly ProcessError InvalidGrant =
        new(ErrorCode.InvalidGrant, "grant is invalid", ResultCode.BadRequest);

    public static readonly ProcessError UnauthorizedForRedirectUri =
        new(ErrorCode.UnauthorizedClient, "client is unauthorized for redirect_uri", ResultCode.BadRequest);

    public static readonly ProcessError ConsentRequired =
        new(ErrorCode.ConsentRequired, "consent is required", ResultCode.BadRequest);

    public static readonly ProcessError ScopeExceedsConsentedScope =
        new(ErrorCode.InvalidScope, "scope exceeds consented scope", ResultCode.BadRequest);

    public static readonly ProcessError LoginRequired =
        new(ErrorCode.LoginRequired, "login required", ResultCode.BadRequest);

    public static readonly ProcessError InvalidScope =
        new(ErrorCode.InvalidRequest, "scope is missing", ResultCode.BadRequest);
}