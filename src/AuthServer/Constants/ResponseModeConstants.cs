namespace AuthServer.Constants;

public static class ResponseModeConstants
{
    public const string Query = "query";
    public const string Fragment = "fragment";
    public const string FormPost = "form_post";

    public static readonly string[] ResponseModes = [Query, Fragment, FormPost];
}