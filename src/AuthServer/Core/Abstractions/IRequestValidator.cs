using AuthServer.Core.Request;

namespace AuthServer.Core.Abstractions;
internal interface IRequestValidator<in TRequest, TValidatedRequest>
    where TRequest : class
    where TValidatedRequest : class
{
    /// <summary>
    /// Validates a request. If an error occurs a <see cref="ProcessError"/>> is returned
    /// and if validation is successful, then a <see cref="TValidatedRequest"/> is returned.
    /// </summary>
    /// <typeparam name="TRequest"></typeparam>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns><see cref="ProcessResult{TValue,TError}"/></returns>
    Task<ProcessResult<TValidatedRequest, ProcessError>> Validate(TRequest request, CancellationToken cancellationToken);
}