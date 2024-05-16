namespace AuthServer.Core.RequestProcessing;

public enum ResultCode
{
    Ok = 200,
    Redirect = 302,
    BadRequest = 400,
    Unauthorized = 401,
    ServerError = 500
}