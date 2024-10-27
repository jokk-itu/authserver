using System.Security.Claims;

namespace AuthServer.Authentication.Abstractions;
public interface IUserClaimService
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="subjectIdentifier"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<IEnumerable<Claim>> GetClaims(string subjectIdentifier, CancellationToken cancellationToken);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="subjectIdentifier"></param>
    /// <param name="cancellation"></param>
    /// <returns></returns>
    Task<string> GetUsername(string subjectIdentifier, CancellationToken cancellation);
}