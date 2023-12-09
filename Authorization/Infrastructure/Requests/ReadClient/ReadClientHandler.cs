using System.Net;
using Domain;
using Domain.Enums;
using Infrastructure.Builders.Token.Abstractions;
using Infrastructure.Builders.Token.RegistrationToken;
using Infrastructure.Helpers;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Requests.ReadClient;
public class ReadClientHandler : IRequestHandler<ReadClientQuery, ReadClientResponse>
{
  private readonly IdentityContext _identityContext;
  private readonly ITokenBuilder<RegistrationTokenArguments> _tokenBuilder;

  public ReadClientHandler(
    IdentityContext identityContext,
    ITokenBuilder<RegistrationTokenArguments> tokenBuilder)
  {
    _identityContext = identityContext;
    _tokenBuilder = tokenBuilder;
  }

  public async Task<ReadClientResponse> Handle(ReadClientQuery request, CancellationToken cancellationToken)
  {
    var client = await _identityContext
      .Set<Client>()
      .Where(x => x.Id == request.ClientId)
      .Where(x => x.ClientTokens
        .AsQueryable()
        .Where(y => y.Reference == request.Token)
        .Where(y => y.RevokedAt == null)
        .OfType<RegistrationToken>()
        .Any())
      .Include(x => x.Scopes)
      .Include(x => x.Contacts)
      .Include(x => x.GrantTypes)
      .Include(x => x.RedirectUris)
      .SingleAsync(cancellationToken: cancellationToken);

    var plainTextSecret = client.TokenEndpointAuthMethod == TokenEndpointAuthMethod.None
      ? null
      : CryptographyHelper.GetRandomString(32);

    client.Secret = client.TokenEndpointAuthMethod == TokenEndpointAuthMethod.None
      ? null
      : BCrypt.HashPassword(plainTextSecret, BCrypt.GenerateSalt());

    client.ClientTokens.Single().RevokedAt = DateTime.UtcNow;
    var registrationToken = await _tokenBuilder.BuildToken(new RegistrationTokenArguments
    {
      Client = client
    });
    await _identityContext.SaveChangesAsync(cancellationToken: cancellationToken);

    return new ReadClientResponse(HttpStatusCode.OK)
    {
      ClientId = client.Id,
      ClientSecret = plainTextSecret,
      ClientName = client.Name,
      ApplicationType = client.ApplicationType.ToString(),
      SubjectType = client.SubjectType.ToString(),
      TokenEndpointAuthMethod = client.TokenEndpointAuthMethod.ToString(),
      RegistrationAccessToken = registrationToken,
      ClientUri = client.ClientUri,
      TosUri = client.TosUri,
      PolicyUri = client.PolicyUri,
      LogoUri = client.LogoUri,
      InitiateLoginUri = client.InitiateLoginUri,
      DefaultMaxAge = client.DefaultMaxAge
    };
  }
}