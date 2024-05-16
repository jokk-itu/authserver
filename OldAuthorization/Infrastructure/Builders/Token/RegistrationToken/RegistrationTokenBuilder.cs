using Application;
using Infrastructure.Builders.Token.Abstractions;

namespace Infrastructure.Builders.Token.RegistrationToken;
public class RegistrationTokenBuilder : ITokenBuilder<RegistrationTokenArguments>
{
  private readonly IdentityContext _identityContext;
  private readonly IdentityConfiguration _identityConfiguration;

  public RegistrationTokenBuilder(
    IdentityContext identityContext,
    IdentityConfiguration identityConfiguration)
  {
    _identityContext = identityContext;
    _identityConfiguration = identityConfiguration;
  }

  public async Task<string> BuildToken(RegistrationTokenArguments arguments)
  {
    var now = DateTime.UtcNow;
    var registrationToken = new Domain.RegistrationToken
    {
      Client = arguments.Client,
      Audience = arguments.Client.Id,
      Issuer = _identityConfiguration.Issuer,
      NotBefore = now,
      IssuedAt = now
    };

    await _identityContext
      .Set<Domain.RegistrationToken>()
      .AddAsync(registrationToken);

    return registrationToken.Reference;
  }
}
