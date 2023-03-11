using System.Net;
using System.Security;
using Domain;
using Domain.Constants;
using Infrastructure.Decoders.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Requests.ReadClient;
public class ReadClientHandler : IRequestHandler<ReadClientQuery, ReadClientResponse>
{
  private readonly IdentityContext _identityContext;
  private readonly ITokenDecoder _tokenDecoder;

  public ReadClientHandler(
    IdentityContext identityContext,
    ITokenDecoder tokenDecoder)
  {
    _identityContext = identityContext;
    _tokenDecoder = tokenDecoder;
  }

  public async Task<ReadClientResponse> Handle(ReadClientQuery request, CancellationToken cancellationToken)
  {
    var configurationToken = _tokenDecoder.DecodeSignedToken(request.Token);
    if (configurationToken is null)
      throw new SecurityException("configuration token is invalid after validation");

    var clientId = configurationToken.Claims.Single(x => x.Type == ClaimNameConstants.ClientId).Value;

    var client = await _identityContext
      .Set<Client>()
      .SingleAsync(x => x.Id == clientId, cancellationToken: cancellationToken);

    return new ReadClientResponse(HttpStatusCode.OK)
    {
    };
  }
}
