namespace AuthServer.Authorize.Abstractions;

internal interface IAuthorizeProcessor
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<string> Process(AuthorizeValidatedRequest request, CancellationToken cancellationToken);
}