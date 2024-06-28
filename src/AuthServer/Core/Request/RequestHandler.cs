namespace AuthServer.Core.Request;
public abstract class RequestHandler<TRequest, TValidatedRequest, TResponse> : IRequestHandler<TRequest, TResponse>
    where TRequest : notnull
    where TValidatedRequest : notnull
{
    /// <inheritdoc cref="IRequestProcessor{TRequest,TResponse}"/>
    public async Task<ProcessResult<TResponse, ProcessError>> Handle(TRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await ValidateRequest(request, cancellationToken);
        return await validationResult.Match(
            validatedRequest => ProcessRequest(validatedRequest, cancellationToken),
            error => Task.FromResult(new ProcessResult<TResponse, ProcessError>(error)));
    }

    /// <summary>
    /// Assumes that <typeparam name="TValidatedRequest"></typeparam> has been validated.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected abstract Task<ProcessResult<TResponse, ProcessError>> ProcessRequest(TValidatedRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Assumes that <typeparam name="TRequest"></typeparam> is raw from the endpoint.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected abstract Task<ProcessResult<TValidatedRequest, ProcessError>> ValidateRequest(TRequest request, CancellationToken cancellationToken);
}