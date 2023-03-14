using System.Net;
using Domain;
using Infrastructure.Builders.Abstractions;
using Infrastructure.Helpers;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Requests.CreateResource;
public class CreateResourceHandler : IRequestHandler<CreateResourceCommand, CreateResourceResponse>
{
  private readonly IdentityContext _identityContext;
  private readonly ITokenBuilder _tokenBuilder;

  public CreateResourceHandler(
    IdentityContext identityContext,
    ITokenBuilder tokenBuilder)
  {
    _identityContext = identityContext;
    _tokenBuilder = tokenBuilder;
  }

  public async Task<CreateResourceResponse> Handle(CreateResourceCommand request, CancellationToken cancellationToken)
  {
    var scopes = await _identityContext
      .Set<Scope>()
      .Where(x => request.Scopes.Contains(x.Name))
      .ToListAsync(cancellationToken: cancellationToken);

    var resource = new Resource
    {
      Scopes = scopes,
      Name = request.ResourceName,
      Secret = CryptographyHelper.GetRandomString(32)
    };

    await _identityContext
      .Set<Resource>()
      .AddAsync(resource, cancellationToken);

    await _identityContext.SaveChangesAsync(cancellationToken);
    var resourceRegistrationAccessToken = _tokenBuilder.BuildResourceRegistrationAccessToken(resource.Id);
    return new CreateResourceResponse(HttpStatusCode.Created)
    {
      ResourceId = resource.Id,
      ResourceName = resource.Name,
      ResourceRegistrationAccessToken = resourceRegistrationAccessToken,
      ResourceSecret = resource.Secret,
      Scope = string.Join(' ', resource.Scopes)
    };
  }
}
