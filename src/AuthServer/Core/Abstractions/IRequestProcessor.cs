namespace AuthServer.Core.Request;
internal interface IRequestProcessor<in TValidatedRequest, TResponse>
    where TValidatedRequest : notnull
	where TResponse : notnull
{
	/// <summary>
	/// 
	/// </summary>
	/// <param name="request"></param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	Task<TResponse> Process(TValidatedRequest request, CancellationToken cancellationToken);
}