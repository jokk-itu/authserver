namespace AuthServer.Userinfo;
internal interface IUserinfoProcessor
{
    Task<string> GetUserinfo(UserinfoValidatedRequest request, CancellationToken cancellationToken);
}