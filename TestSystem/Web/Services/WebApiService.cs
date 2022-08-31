namespace WebApp.Services;

public class WebApiService
{
  private readonly HttpClient _httpClient;

  public WebApiService(HttpClient httpClient)
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