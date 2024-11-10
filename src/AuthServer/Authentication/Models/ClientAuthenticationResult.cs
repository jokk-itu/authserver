namespace AuthServer.Authentication.Models;
internal record ClientAuthenticationResult(string? ClientId, bool IsAuthenticated);