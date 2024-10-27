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

    public static readonly ProcessError StateWithoutPostLogoutRedirectUri =
        new(ErrorCode.InvalidRequest, "state provided without post_logout_redirect_uri", ResultCode.BadRequest);

    public static readonly ProcessError PostLogoutRedirectUriWithoutState =
        new(ErrorCode.InvalidRequest, "post_logout_redirect_uri provided without state", ResultCode.BadRequest);

    public static readonly ProcessError PostLogoutRedirectUriWithoutClientIdOrIdTokenHint =
        new(ErrorCode.InvalidRequest, "post_logout_redirect_uri provided without client_id or id_token_hint", ResultCode.BadRequest);

    public static readonly ProcessError UnauthorizedClientForPostLogoutRedirectUri =
        new(ErrorCode.InvalidRequest, "client is not authorized for the post_logout_redirect_uri", ResultCode.BadRequest);

    public static readonly ProcessError InteractionRequired =
        new(ErrorCode.InteractionRequired, "end-user must interact with logout page", ResultCode.Redirect);
}