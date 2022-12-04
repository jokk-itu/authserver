using Microsoft.Extensions.Options;

namespace WorkerService;
public class ConfigureWeatherOptions : IConfigureOptions<WeatherOptions>
{
  private readonly IConfiguration _configuration;

  public ConfigureWeatherOptions(IConfiguration configuration)
  {
    _configuration = configuration;
  }
  public void Configure(WeatherOptions options)
  {
    var weatherService = _configuration.GetSection("WeatherService");
    var baseUrl = weatherService.GetValue<string>(nameof(WeatherOptions.BaseUrl));
    options.BaseUrl = baseUrl;
  }
}
