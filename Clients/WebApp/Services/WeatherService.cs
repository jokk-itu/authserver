using App.Contracts;

namespace App.Services;

public class WeatherService
{
  private readonly HttpClient _httpClient;

  public WeatherService(HttpClient httpClient)
  {
    _httpClient = httpClient;
  }

  public async Task<IEnumerable<WeatherDto>> GetSecretAsync()
  {
    var response = await _httpClient.GetFromJsonAsync<IEnumerable<WeatherDto>>("api/weather");
    return response;
  }
}