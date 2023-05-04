namespace Infrastructure.Services.Abstract;
public interface IClaimService
{
    Task<IDictionary<string, string>> GetClaimsFromConsentGrant(string userId, string clientId, CancellationToken cancellationToken = default);
}