using System.Security.Claims;
using System.Text.Json;
using AuthServer.Authentication.Abstractions;
using AuthServer.Constants;

namespace AuthServer.Tests.Core;
public class UserClaimService : IUserClaimService
{
    public Task<IEnumerable<Claim>> GetClaims(string subjectIdentifier, CancellationToken cancellationToken)
    {
        return Task.FromResult<IEnumerable<Claim>>(
        [
            new Claim(ClaimNameConstants.Name, UserConstants.Name),
            new Claim(ClaimNameConstants.GivenName, UserConstants.GivenName),
            new Claim(ClaimNameConstants.MiddleName, UserConstants.MiddleName),
            new Claim(ClaimNameConstants.FamilyName, UserConstants.FamilyName),
            new Claim(ClaimNameConstants.Address, UserConstants.Address),
            new Claim(ClaimNameConstants.NickName, UserConstants.NickName),
            new Claim(ClaimNameConstants.PreferredUsername, UserConstants.PreferredUsername),
            new Claim(ClaimNameConstants.Profile, UserConstants.Profile),
            new Claim(ClaimNameConstants.Picture, UserConstants.Picture),
            new Claim(ClaimNameConstants.Website, UserConstants.Website),
            new Claim(ClaimNameConstants.Email, UserConstants.Email),
            new Claim(ClaimNameConstants.EmailVerified, UserConstants.EmailVerified),
            new Claim(ClaimNameConstants.Gender, UserConstants.Gender),
            new Claim(ClaimNameConstants.Birthdate, UserConstants.Birthdate),
            new Claim(ClaimNameConstants.ZoneInfo, UserConstants.ZoneInfo),
            new Claim(ClaimNameConstants.Locale, UserConstants.Locale),
            new Claim(ClaimNameConstants.PhoneNumber, UserConstants.PhoneNumber),
            new Claim(ClaimNameConstants.PhoneNumberVerified, UserConstants.PhoneNumberVerified),
            new Claim(ClaimNameConstants.UpdatedAt, UserConstants.UpdatedAt),
            new Claim(ClaimNameConstants.Roles, JsonSerializer.Serialize(UserConstants.Roles))
        ]);
    }

    public Task<string> GetUsername(string subjectIdentifier, CancellationToken cancellation)
    {
        return Task.FromResult(UserConstants.Username);
    }
}