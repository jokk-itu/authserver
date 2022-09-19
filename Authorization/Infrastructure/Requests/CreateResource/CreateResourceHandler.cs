using System.Net;
using Application.Validation;
using Domain;
using Infrastructure.Builders.Abstractions;
using Infrastructure.Helpers;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Requests.CreateResource;
public class CreateResourceHandler : IRequestHandler<CreateResourceCommand, CreateResourceResponse>
{
  private readonly IdentityContext _identityContext;
  private readonly IValidator<CreateResourceCommand> _validator;
  private readonly ITokenBuilder _tokenBuilder;

  public CreateResourceHandler(
    IdentityContext identityContext, 
    IValidator<CreateResourceCommand> validator,
    ITokenBuilder tokenBuilder)
  {
    _identityContext = identityContext;
    _validator = validator;
    _tokenBuilder = tokenBuilder;
  }

  public async Task<CreateResourceResponse> Handle(CreateResourceCommand request, CancellationToken cancellationToken)
  {
    var validationResult = await _validator.IsValidAsync(request);
    if (validationResult.IsError())
      return new CreateResourceResponse(validationResult.ErrorCode, validationResult.ErrorDescription, validationResult.StatusCode);

    var scopes = await _identityContext
      .Set<Scope>()
      .Where(x => request.Scopes.Contains(x.Name))
      .ToListAsync(cancellationToken: cancellationToken);

    var resource = new Resource
    {
      Id = Guid.NewGuid().ToString(),
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
