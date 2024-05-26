namespace AuthServer.Constants;

public static class ResponseModeConstants
{
    public const string FormPost = "form_post";
    public const string FormPostJwt = "form_post.jwt";
    public const string Jwt = "jwt";

    public static readonly string[] ResponseModes = [FormPost, FormPostJwt, Jwt];
}