namespace AuthServer.EndSession;
public record EndSessionUser(string? SubjectIdentifier, bool LogoutAtIdentityProvider);