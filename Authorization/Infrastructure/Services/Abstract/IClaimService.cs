namespace Infrastructure.Services.Abstract;
public interface IClaimService
{
  Task<IEnumerable<KeyValuePair<string, object>>> GetClaimsFromConsentGrant(string userId, string clientId,
    CancellationToken cancellationToken = default);
}