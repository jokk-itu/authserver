using System.Net;
using Domain;
using Infrastructure.Builders.Abstractions;
using MediatR;

namespace Infrastructure.Requests.CreateScope;
public class CreateScopeHandler : IRequestHandler<CreateScopeCommand, CreateScopeResponse>
{
  private readonly IdentityContext _identityContext;
  private readonly ITokenBuilder _tokenBuilder;

  public CreateScopeHandler(
    IdentityContext identityContext,
    ITokenBuilder tokenBuilder)
  {
    _identityContext = identityContext;
    _tokenBuilder = tokenBuilder;
  }

  public async Task<CreateScopeResponse> Handle(CreateScopeCommand request, CancellationToken cancellationToken)
  {
    var scope = new Scope
    {
      Name = request.ScopeName
    };
    await _identityContext
      .Set<Scope>()
      .AddAsync(scope, cancellationToken: cancellationToken);

    await _identityContext.SaveChangesAsync(cancellationToken: cancellationToken);
    var token = _tokenBuilder.BuildScopeRegistrationAccessToken(scope.Id.ToString());
    return new CreateScopeResponse(HttpStatusCode.Created)
    {
      Id = scope.Id,
      ScopeName = scope.Name,
      ScopeRegistrationAccessToken = token
    };
  }
}