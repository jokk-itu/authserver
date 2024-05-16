using System.Security.Claims;

namespace AuthServer.Core.Abstractions;
public interface IUserClaimService
{
    Task<IEnumerable<Claim>> GetClaims(string subjectIdentifier, CancellationToken cancellationToken);
}