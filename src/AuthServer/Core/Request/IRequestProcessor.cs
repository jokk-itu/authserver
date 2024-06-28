namespace AuthServer.Core.Request;
internal interface IRequestProcessor<in TValidatedRequest, TResponse>
    where TValidatedRequest : notnull
	where TResponse : notnull
{
	Task<TResponse> Process(TValidatedRequest request, CancellationToken cancellationToken);
}