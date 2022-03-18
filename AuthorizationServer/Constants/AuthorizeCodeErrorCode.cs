namespace AuthorizationServer.Constants;

public static class AuthorizeCodeErrorCode
{
    public const string UnsupportedResponseType = "unsupported_response_type\nThe authorization server does not support obtaining an authorization code using this method.";
    public const string InvalidRequest = "invalid_request\n The request is missing a required parameter, includes an invalid parameter value, includes a parameter more than once, or is otherwise malformed.";
    public const string InvalidScope = "invalid_scope\nThe requested scope is invalid, unknown, or malformed.";
}