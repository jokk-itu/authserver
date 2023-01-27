using Microsoft.Extensions.Options;

namespace WorkerService;
public class ConfigureWorkerOptions : IConfigureOptions<WorkerOptions>
{
  private readonly IConfiguration _configuration;

  public ConfigureWorkerOptions(IConfiguration configuration)
  {
    _configuration = configuration;
  }

  public void Configure(WorkerOptions options)
  {
    var identity = _configuration.GetSection("Identity");
    var clientId = identity.GetValue<string>(nameof(WorkerOptions.ClientId));
    var clientSecret = identity.GetValue<string>(nameof(WorkerOptions.ClientSecret));
    var authority = identity.GetValue<string>(nameof(WorkerOptions.Authority));
    var tokenEndpoint = identity.GetValue<string>(nameof(WorkerOptions.TokenEndpoint));
    options.ClientId = clientId;
    options.ClientSecret = clientSecret;
    options.Authority = authority;
    options.TokenEndpoint = tokenEndpoint;
  }
}
