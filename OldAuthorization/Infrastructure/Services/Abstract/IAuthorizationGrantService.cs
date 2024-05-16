using static Infrastructure.Services.AuthorizationGrantService;

namespace Infrastructure.Services.Abstract;
public interface IAuthorizationGrantService
{
  Task<AuthorizationGrantResult> CreateAuthorizationGrant(CreateAuthorizationGrantArguments arguments,
    CancellationToken cancellationToken);

  Task<AuthorizationGrantResult> UpdateAuthorizationGrant(UpdateAuthorizationGrantArguments arguments,
    CancellationToken cancellationToken);
}
