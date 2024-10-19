namespace AuthServer.Core.Models;
internal record ClientAuthenticationResult(string? ClientId, bool IsAuthenticated);