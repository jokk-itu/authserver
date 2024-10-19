using AuthServer.Core;
using AuthServer.Core.Request;

namespace AuthServer.Revocation;

internal static class RevocationError
{
    public static readonly ProcessError UnsupportedTokenType = 
        new(ErrorCode.UnsupportedTokenType, "provided token_type_hint is unsupported", ResultCode.BadRequest);

    public static readonly ProcessError EmptyToken = 
        new(ErrorCode.InvalidRequest, "token must not be null or empty", ResultCode.BadRequest);

    public static readonly ProcessError MultipleOrNoneClientMethod = 
        new(ErrorCode.InvalidClient, "only one client authentication method must be used", ResultCode.BadRequest);

    public static readonly ProcessError InvalidClient =
        new(ErrorCode.InvalidClient, "client could not be authenticated", ResultCode.BadRequest);

    public static readonly ProcessError ClientIdDoesNotMatchToken =
        new(ErrorCode.InvalidClient, "client_id does not match token", ResultCode.BadRequest);
}