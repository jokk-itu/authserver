using AuthServer.Core;
using AuthServer.Core.Request;

namespace AuthServer.EndSession;
internal static class EndSessionError
{
    public static readonly ProcessError InvalidIdToken =
        new(ErrorCode.InvalidRequest, "invalid id_token_hint", ResultCode.BadRequest);

    public static readonly ProcessError MismatchingClientId =
        new(ErrorCode.InvalidRequest, "client_id does not match id_token_hint", ResultCode.BadRequest);

    public static readonly ProcessError InvalidClientId =
        new(ErrorCode.InvalidRequest, "invalid client_id", ResultCode.BadRequest);

    public static readonly ProcessError InvalidState =
        new(ErrorCode.InvalidRequest, "invalid state", ResultCode.BadRequest);

    public static readonly ProcessError InvalidPostLogoutRedirectUri =
        new(ErrorCode.InvalidRequest, "invalid post_logout_redirect_uri", ResultCode.BadRequest);

    // Custom internal error
    public static readonly ProcessError InteractionRequired =
        new(ErrorCode.InteractionRequired, "end-user must interact with logout page", ResultCode.Redirect);
}