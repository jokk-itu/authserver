namespace AuthServer.Register.Abstractions;

internal interface IPostRegisterProcessor
{
    Task<PostRegisterResponse> Register(PostRegisterValidatedRequest postRegisterValidatedRequest, CancellationToken cancellationToken);
}