namespace WebApp.Services;

public class WeatherService
{
  private readonly HttpClient _httpClient;

  public WeatherService(HttpClient httpClient)
  {
    _httpClient = httpClient;
  }

  public async Task<string> GetSecretAsync()
  {
    return await _httpClient.GetStringAsync("api/secret");
  }

  public async Task<string> GetAnonymousAsync()
  {
    return await _httpClient.GetStringAsync("api/anonymous");
  }
}