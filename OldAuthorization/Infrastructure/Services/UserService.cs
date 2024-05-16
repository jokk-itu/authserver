using Domain;
using Infrastructure.Helpers;
using Infrastructure.Services.Abstract;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;
public class UserService : IUserService
{
  private readonly IdentityContext _identityContext;

  public UserService(
    IdentityContext identityContext)
  {
    _identityContext = identityContext;
  }

  public async Task<bool> IsValid(string username, string password, CancellationToken cancellationToken = default)
  {
    var user = await _identityContext.Set<User>().SingleOrDefaultAsync(x => x.UserName == username, cancellationToken: cancellationToken);
    return user is not null && BCrypt.CheckPassword(password, user.Password);
  }
}