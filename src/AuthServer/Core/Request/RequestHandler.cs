using System.Diagnostics;
using AuthServer.Core.Abstractions;
using AuthServer.Metrics.Abstractions;

namespace AuthServer.Core.Request;
internal abstract class RequestHandler<TRequest, TValidatedRequest, TResponse> : IRequestHandler<TRequest, TResponse>
    where TRequest : class
    where TValidatedRequest : class
    where TResponse : class
{
    private readonly IMetricService _metricService;

    protected RequestHandler(IMetricService metricService)
    {
        _metricService = metricService;
    }

    /// <inheritdoc cref="IRequestProcessor{TRequest,TResponse}"/>
    public async Task<ProcessResult<TResponse, ProcessError>> Handle(TRequest request, CancellationToken cancellationToken)
    {
        using var activity = _metricService.ActivitySource.StartActivity();
        var validationResult = await ValidateRequest(request, cancellationToken);

        return await validationResult.Match(
            validatedRequest =>
            {
                activity?.AddEvent(new ActivityEvent("Request validation succeeded"));
                return ProcessRequest(validatedRequest, cancellationToken);
            },
            error =>
            {
                activity?.AddEvent(new ActivityEvent("Request validation failed"));
                return Task.FromResult(new ProcessResult<TResponse, ProcessError>(error));
            });
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