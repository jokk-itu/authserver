using System.Net;
using Application.Validation;
using Domain;
using Domain.Enums;
using Domain.Extensions;
using Infrastructure.Builders.Abstractions;
using Infrastructure.Helpers;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Requests.CreateClient;
public class CreateClientHandler : IRequestHandler<CreateClientCommand, CreateClientResponse>
{
  private readonly IValidator<CreateClientCommand> _createClientValidator;
  private readonly IdentityContext _identityContext;
  private readonly ITokenBuilder _tokenBuilder;

  public CreateClientHandler(
    IValidator<CreateClientCommand> createClientValidator,
    IdentityContext identityContext,
    ITokenBuilder tokenBuilder)
  {
    _createClientValidator = createClientValidator;
    _identityContext = identityContext;
    _tokenBuilder = tokenBuilder;
  }

  public async Task<CreateClientResponse> Handle(CreateClientCommand request, CancellationToken cancellationToken)
  {
    var validationResult = await _createClientValidator.ValidateAsync(request, cancellationToken);
    if (validationResult.IsError())
      return new CreateClientResponse(validationResult.ErrorCode, validationResult.ErrorDescription,
        validationResult.StatusCode);

    var scopes = await _identityContext
      .Set<Scope>()
      .Where(x => request.Scopes.Contains(x.Name))
      .ToListAsync(cancellationToken: cancellationToken);

    var grantTypes = await _identityContext
      .Set<GrantType>()
      .Where(x => request.GrantTypes.Contains(x.Name))
      .ToListAsync(cancellationToken: cancellationToken);

    var redirectUris = request.RedirectUris
      .Select(x => new RedirectUri { Uri = x })
      .ToList();

    var responseTypes = await _identityContext
      .Set<ResponseType>()
      .Where(x => request.ResponseTypes.Contains(x.Name))
      .ToListAsync(cancellationToken: cancellationToken);

    var contacts = request.Contacts?
      .Select(email => new Contact
      {
        Email = email
      }).ToList() ?? new List<Contact>();

    var client = new Client
    {
      Id = Guid.NewGuid().ToString(),
      Name = request.ClientName,
      Secret = CryptographyHelper.GetRandomString(32),
      Scopes = scopes,
      RedirectUris = redirectUris,
      GrantTypes = grantTypes,
      ClientProfile = ClientProfile.WebApplication,
      ClientType = ClientType.Confidential,
      ResponseTypes = responseTypes,
      TokenEndpointAuthMethod = request.TokenEndpointAuthMethod.GetEnum<TokenEndpointAuthMethod>(),
      PolicyUri = request.PolicyUri,
      Contacts = contacts,
      SubjectType = request.SubjectType.GetEnum<SubjectType>(),
      TosUri = request.TosUri
    };
    await _identityContext
      .Set<Client>()
      .AddAsync(client, cancellationToken: cancellationToken);

    await _identityContext.SaveChangesAsync(cancellationToken: cancellationToken);

    return new CreateClientResponse(HttpStatusCode.Created)
    {
      ApplicationType = request.ApplicationType,
      GrantTypes = client.GrantTypes.Select(x => x.Name).ToList(),
      ClientId = client.Id,
      ClientName = client.Name,
      ClientSecret = client.Secret,
      Scope = string.Join(' ', client.Scopes),
      RedirectUris = client.RedirectUris.Select(x => x.Uri).ToList(),
      SubjectType = request.SubjectType,
      TosUri = client.TosUri,
      Contacts = client.Contacts.Select(x => x.Email).ToList(),
      PolicyUri = client.PolicyUri,
      TokenEndpointAuthMethod = request.TokenEndpointAuthMethod,
      ResponseTypes = client.ResponseTypes.Select(x => x.Name).ToList(),
      RegistrationAccessToken = _tokenBuilder.BuildClientRegistrationAccessToken(client.Id)
    };
  }
}
