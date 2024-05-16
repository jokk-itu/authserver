namespace AuthServer.Constants;

public static class ResponseModeConstants
{
    public const string Query = "query";
    public const string Fragment = "fragment";
    public const string FormPost = "form_post";
    public const string QueryJwt = "query.jwt";
    public const string FragmentJwt = "fragment.jwt";
    public const string FormPostJwt = "form_post.jwt";
    public const string Jwt = "jwt";

    public static readonly string[] ResponseModes = [Query, Fragment, FormPost, QueryJwt, FragmentJwt, FormPostJwt, Jwt];
}