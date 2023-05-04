using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Infrastructure.PipelineBehaviors;
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
where TRequest : IRequest<TResponse>
{
  private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

  public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
  {
    _logger = logger;
  }

  public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
  {
    var stopWatch = Stopwatch.StartNew();
    try
    {
      var response = await next();
      stopWatch.Stop();
      _logger.LogInformation("Request took {ElapsedTime} ms", stopWatch.ElapsedMilliseconds);
      return response;
    }
    catch (Exception e)
    {
      stopWatch.Stop();
      _logger.LogError(e, "Error occurred during request, took {ElapsedTime} ms", stopWatch.ElapsedMilliseconds);
      throw;
    }
  }
}