using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Infrastructure.DelegatingHandlers;
public class PerformanceDelegatingHandler : DelegatingHandler
{
  private readonly ILogger<PerformanceDelegatingHandler> _logger;

  public PerformanceDelegatingHandler(ILogger<PerformanceDelegatingHandler> logger)
  {
    _logger = logger;
  }

  protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
    CancellationToken cancellationToken)
  {
    var watch = Stopwatch.StartNew();
    var response = await base.SendAsync(request, cancellationToken);
    var time = watch.ElapsedMilliseconds;
    try
    {
      response.EnsureSuccessStatusCode();
      _logger.LogInformation("HTTP Request to {RequestUri} completed with {StatusCode}, took {ElapsedTime} ms",
        request.RequestUri, response.StatusCode, time);
    }
    catch (Exception e)
    {
      _logger.LogError(e, "HTTP Request to {RequestUri} completed with {StatusCode}, took {ElapsedTime} ms",
        request.RequestUri, response.StatusCode, time);
    }
    
    return response;
  }
}