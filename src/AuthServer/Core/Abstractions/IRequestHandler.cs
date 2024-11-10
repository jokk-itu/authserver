using AuthServer.Core.Request;

namespace AuthServer.Core.Abstractions;

internal interface IRequestHandler<in TRequest, TResponse>
	where TRequest : class
	where TResponse : class
{
	/// <summary>
	/// Processes a request and returns a successful response or an error response,
	/// which can be transformed to an HTTP response.
	/// </summary>
	/// <param name="request"></param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	Task<ProcessResult<TResponse, ProcessError>> Handle(TRequest request, CancellationToken cancellationToken);
}