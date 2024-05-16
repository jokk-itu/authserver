namespace AuthServer.Core.RequestProcessing;
internal interface IRequestProcessor<in TRequest, TResponse>
    where TRequest : notnull
{
    /// <summary>
    /// Processes a request and returns a successful response or an error response,
    /// which can be transformed to an HTTP response.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ProcessResult<TResponse, ProcessError>> Process(TRequest request, CancellationToken cancellationToken);
}