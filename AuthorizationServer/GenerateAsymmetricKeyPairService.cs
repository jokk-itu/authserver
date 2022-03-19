using AuthorizationServer.Repositories;

namespace AuthorizationServer;

public class GenerateAsymmetricKeyPairService : BackgroundService
{
    private readonly AsyncServiceScope _scope;

    public GenerateAsymmetricKeyPairService(IServiceProvider serviceProvider)
    {
        _scope = serviceProvider.CreateAsyncScope();
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await base.StopAsync(cancellationToken);
        await _scope.DisposeAsync();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var keypairManager = _scope.ServiceProvider.GetRequiredService<AsymmetricKeyPairManager>();
        while (!stoppingToken.IsCancellationRequested)
        {
            //Get the top three keys
            //If the there are no keys then create a present and a future
            //If there is an old key, then create a present and a future
            //If there is a present key, then create a future
            //If there is a present and a future key, then do nothing
            
            
            await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
        }
    }
}