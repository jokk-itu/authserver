namespace AuthServer.Core.Request;

public enum ResultCode
{
    Ok = 200,
    Redirect = 302,
    BadRequest = 400,
    Unauthorized = 401,
    Forbidden = 403,
    ServerError = 500
}