using IdentityModel.Client;
using Microsoft.Extensions.Options;
using WorkerService.Services.Abstract;

namespace WorkerService.Services;
public class TokenService : ITokenService
{
  private int _expiresIn;
  private DateTime _fetchedAt = DateTime.UtcNow;
  private string? _token;
  private readonly IOptions<WorkerOptions> _options;
  private readonly HttpClient _httpClient;
  private readonly ILogger<TokenService> _logger;
  private readonly IOptionsMonitor<WeatherOptions> _weatherOptions;

  public TokenService(
    IServiceProvider serviceProvider,
    IHttpClientFactory httpClientFactory,
    ILogger<TokenService> logger,
    IOptionsMonitor<WeatherOptions> weatherOptions)
  {
    using var scope = serviceProvider.CreateScope();
    _options = scope.ServiceProvider.GetRequiredService<IOptionsSnapshot<WorkerOptions>>();
    _httpClient = httpClientFactory.CreateClient(nameof(TokenService));
    _logger = logger;
    _weatherOptions = weatherOptions;
  }

  public async Task<string?> GetToken(string scope, CancellationToken cancellationToken = default)
  {
    if (_fetchedAt.AddSeconds(_expiresIn) > DateTime.UtcNow && _token != null)
    {
      return _token;
    }

    var tokenClientOptions = new TokenClientOptions
    {
      ClientCredentialStyle = ClientCredentialStyle.PostBody,
      ClientId = _options.Value.ClientId,
      ClientSecret = _options.Value.ClientSecret,
      Address = _options.Value.TokenEndpoint,
      Parameters = new Parameters(new Dictionary<string, string>
      {
        { "resource",  _weatherOptions.CurrentValue.BaseUrl }
      })
    };
    var tokenClient = new TokenClient(_httpClient, tokenClientOptions);
    var tokenResponse = await tokenClient.RequestClientCredentialsTokenAsync(scope, cancellationToken: cancellationToken);
    if (tokenResponse.IsError)
    {
      _logger.LogWarning("Error with {Status} occurred during call to token endpoint {Error} {ErrorDescription}",
        tokenResponse.HttpStatusCode, tokenResponse.Error, tokenResponse.ErrorDescription);
      return null;
    }

    _expiresIn = tokenResponse.ExpiresIn;
    _fetchedAt = DateTime.UtcNow;
    _token = tokenResponse.AccessToken;
    return _token;
  }
}
