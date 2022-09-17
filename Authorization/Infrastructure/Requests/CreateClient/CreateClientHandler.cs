using System.Net;
using Application.Validation;
using Domain;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Requests.CreateClient;
public class CreateClientHandler : IRequestHandler<CreateClientCommand, CreateClientResponse>
{
  private readonly IValidator<CreateClientCommand> _createClientValidator;
  private readonly IdentityContext _identityContext;

  public CreateClientHandler(IValidator<CreateClientCommand> createClientValidator, IdentityContext identityContext)
  {
    _createClientValidator = createClientValidator;
    _identityContext = identityContext;
  }

  public async Task<CreateClientResponse> Handle(CreateClientCommand request, CancellationToken cancellationToken)
  {
    var validationResult = await _createClientValidator.IsValidAsync(request);
    if (validationResult.IsError())
      return new CreateClientResponse(validationResult.ErrorCode, validationResult.ErrorDescription,
        validationResult.StatusCode);

    var scopes = await _identityContext
      .Set<Scope>()
      .IgnoreAutoIncludes()
      .Where(x => request.Scopes.Contains(x.Name))
      .ToListAsync(cancellationToken: cancellationToken);

    var grants = await _identityContext
      .Set<Grant>()
      .IgnoreAutoIncludes()
      .Where(x => request.GrantTypes.Contains(x.Name))
      .ToListAsync(cancellationToken: cancellationToken);

    var redirectUris = request.RedirectUris
      .Select(x => new RedirectUri { Uri = x })
      .ToList();

    var client = new Client
    {
      Id = Guid.NewGuid().ToString(),
      Name = request.ClientName,
      Secret = request.ClientSecret,
      Scopes = scopes,
      RedirectUris = redirectUris,
      Grants = grants,
      ClientProfile = ClientProfile.WebApplication,
      ClientType = ClientType.Confidential
    };
    await _identityContext
      .Set<Client>()
      .AddAsync(client, cancellationToken: cancellationToken);

    await _identityContext.SaveChangesAsync(cancellationToken: cancellationToken);

    return new CreateClientResponse(HttpStatusCode.Created)
    {
      ApplicationType = request.ApplicationType,
      GrantTypes = client.Grants.Select(x => x.Name).ToList(),
      ClientId = client.Id,
      ClientName = client.Name,
      ClientSecret = client.Secret,
      Scope = string.Join(' ', client.Scopes),
      RedirectUris = request.RedirectUris
    };
  }
}
