namespace WorkerService.Services.Abstract;

public interface ITokenService
{
  public Task<string?> GetToken(string scope, CancellationToken cancellationToken = default);
}