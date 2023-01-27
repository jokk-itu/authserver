namespace Infrastructure.Services.Abstract;
public interface IUserService
{
  Task<bool> IsValid(string username, string password, CancellationToken cancellationToken = default);
}
