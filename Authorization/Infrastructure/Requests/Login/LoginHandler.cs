using System.Net;
using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Requests.Login;
public class LoginHandler : IRequestHandler<LoginQuery, LoginResponse>
{
  private readonly IdentityContext _identityContext;

  public LoginHandler(
    IdentityContext identityContext)
  {
    _identityContext = identityContext;
  }

  public async Task<LoginResponse> Handle(LoginQuery request, CancellationToken cancellationToken)
  {
    var user = await _identityContext
      .Set<User>()
      .SingleAsync(x => x.UserName == request.Username, cancellationToken: cancellationToken);

    return new LoginResponse(HttpStatusCode.OK)
    {
      UserId = user.Id,
      Name = user.GetName()
    };
  }
}