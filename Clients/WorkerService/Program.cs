using Microsoft.Extensions.Options;
using WorkerService;
using WorkerService.Services;
using WorkerService.Services.Abstract;
using Serilog;

var builder = Host.CreateDefaultBuilder(args);

builder.UseSerilog((context, serviceProvider, configuration) =>
{
  configuration
    .WriteTo.Console();
});

builder.ConfigureServices(services =>
{
  services.AddLogging();
  services.AddSingleton<IConfigureOptions<WorkerOptions>, ConfigureWorkerOptions>();
  services.AddSingleton<IConfigureOptions<WeatherOptions>, ConfigureWeatherOptions>();
  services.AddHttpClient(nameof(Worker), (serviceProvider, httpClient) =>
  {
    using var scope = serviceProvider.CreateScope();
    var weatherOptions = scope.ServiceProvider.GetRequiredService<IOptionsSnapshot<WeatherOptions>>();
    httpClient.BaseAddress = new Uri(weatherOptions.Value.BaseUrl);
  });
  services.AddHttpClient(nameof(TokenService), (serviceProvider, httpClient) =>
  {
    using var scope = serviceProvider.CreateScope();
    var options = scope.ServiceProvider.GetRequiredService<IOptionsSnapshot<WorkerOptions>>();
    httpClient.BaseAddress = new Uri(options.Value.Authority);
  });
  services.AddTransient<ITokenService, TokenService>();
  services.AddHostedService<Worker>();
});

var host = builder.Build();

await host.RunAsync();
