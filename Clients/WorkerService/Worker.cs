using System.Net.Http.Headers;
using WorkerService.Services.Abstract;

namespace WorkerService;

public class Worker : BackgroundService
{
  private readonly ILogger<Worker> _logger;
  private readonly ITokenService _tokenService;
  private readonly HttpClient _httpClient;

  public Worker(
    ILogger<Worker> logger,
    ITokenService tokenService,
    IHttpClientFactory httpClientFactory)
  {
    _logger = logger;
    _tokenService = tokenService;
    _httpClient = httpClientFactory.CreateClient(nameof(Worker));
  }

  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    while (!stoppingToken.IsCancellationRequested)
    {
      await Task.Delay(5000, stoppingToken);
      var token = await _tokenService.GetToken("weather:read", cancellationToken: stoppingToken);
      if (string.IsNullOrEmpty(token))
      {
        continue;
      }
      var request = new HttpRequestMessage(HttpMethod.Get, "api/weather");
      request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
      var response = await _httpClient.SendAsync(request, cancellationToken: stoppingToken);
      var weather = await response.Content.ReadAsStringAsync(cancellationToken: stoppingToken);
      _logger.LogInformation("Get Weather {@prediction}", weather);
    }
  }
}