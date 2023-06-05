using System.Net;
using Domain;
using Domain.Enums;
using Domain.Extensions;
using Infrastructure.Builders.Token.Abstractions;
using Infrastructure.Builders.Token.RegistrationToken;
using Infrastructure.Helpers;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Requests.CreateClient;

public class CreateClientHandler : IRequestHandler<CreateClientCommand, CreateClientResponse>
{
  private readonly IdentityContext _identityContext;
  private readonly ITokenBuilder<RegistrationTokenArguments> _tokenBuilder;

  public CreateClientHandler(
    IdentityContext identityContext,
    ITokenBuilder<RegistrationTokenArguments> tokenBuilder)
  {
    _identityContext = identityContext;
    _tokenBuilder = tokenBuilder;
  }

  public async Task<CreateClientResponse> Handle(CreateClientCommand request, CancellationToken cancellationToken)
  {
    var splitScopes = request.Scope.Split(' ');
    var scopes = await _identityContext
      .Set<Scope>()
      .Where(x => splitScopes.Contains(x.Name))
      .ToListAsync(cancellationToken: cancellationToken);

    var grantTypes = await _identityContext
      .Set<GrantType>()
      .Where(x => request.GrantTypes.Contains(x.Name))
      .ToListAsync(cancellationToken: cancellationToken);

    var redirectUris =  request.RedirectUris
      .Select(x => new RedirectUri { Uri = x, Type = RedirectUriType.AuthorizeRedirectUri })
      .Concat(request.PostLogoutRedirectUris
        .Select(x => new RedirectUri { Uri = x, Type = RedirectUriType.PostLogoutRedirectUri }))
      .ToList();

    var responseTypes = await _identityContext
      .Set<ResponseType>()
      .Where(x => request.ResponseTypes.Contains(x.Name))
      .ToListAsync(cancellationToken: cancellationToken);

    var contacts = request.Contacts
      .Select(email => new Contact
      {
        Email = email
      })
      .ToList();

    long? defaultMaxAge = string.IsNullOrWhiteSpace(request.DefaultMaxAge) ? null : long.Parse(request.DefaultMaxAge);
    var secret = request.TokenEndpointAuthMethod.GetEnum<TokenEndpointAuthMethod>() == TokenEndpointAuthMethod.None
      ? null
      : CryptographyHelper.GetRandomString(32);

    var client = new Client
    {
      Name = request.ClientName,
      Secret = secret,
      ApplicationType = request.ApplicationType.GetEnum<ApplicationType>(),
      Scopes = scopes,
      RedirectUris = redirectUris,
      GrantTypes = grantTypes,
      ResponseTypes = responseTypes,
      TokenEndpointAuthMethod = request.TokenEndpointAuthMethod.GetEnum<TokenEndpointAuthMethod>(),
      Contacts = contacts,
      SubjectType = request.SubjectType.GetEnum<SubjectType>(),
      TosUri = request.TosUri,
      PolicyUri = request.PolicyUri,
      InitiateLoginUri = request.InitiateLoginUri,
      LogoUri = request.LogoUri,
      ClientUri = request.ClientUri,
      DefaultMaxAge = defaultMaxAge,
      BackChannelLogoutUri = request.BackChannelLogoutUri
    };
    await _identityContext
      .Set<Client>()
      .AddAsync(client, cancellationToken: cancellationToken);

    var registrationToken = await _tokenBuilder.BuildToken(new RegistrationTokenArguments
    {
      Client = client
    });

    await _identityContext.SaveChangesAsync(cancellationToken: cancellationToken);

    return new CreateClientResponse(HttpStatusCode.Created)
    {
      ApplicationType = request.ApplicationType,
      GrantTypes = client.GrantTypes.Select(x => x.Name).ToList(),
      ClientId = client.Id,
      ClientName = client.Name,
      ClientSecret = client.Secret,
      Scope = string.Join(' ', client.Scopes.Select(x => x.Name)),
      RedirectUris = client.RedirectUris
        .Where(x => x.Type == RedirectUriType.AuthorizeRedirectUri)
        .Select(x => x.Uri)
        .ToList(),
      PostLogoutRedirectUris = client.RedirectUris
        .Where(x => x.Type == RedirectUriType.PostLogoutRedirectUri)
        .Select(x => x.Uri)
        .ToList(),
      SubjectType = request.SubjectType,
      TosUri = client.TosUri,
      Contacts = client.Contacts.Select(x => x.Email).ToList(),
      PolicyUri = client.PolicyUri,
      TokenEndpointAuthMethod = request.TokenEndpointAuthMethod,
      ResponseTypes = client.ResponseTypes.Select(x => x.Name).ToList(),
      RegistrationAccessToken = registrationToken,
      ClientSecretExpiresAt = 0,
      ClientIdIssuedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
      ClientUri = request.ClientUri,
      DefaultMaxAge = defaultMaxAge,
      LogoUri = request.LogoUri,
      InitiateLoginUri = request.InitiateLoginUri
    };
  }
}