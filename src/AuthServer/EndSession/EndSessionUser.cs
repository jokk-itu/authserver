namespace AuthServer.EndSession;
internal record EndSessionUser(string? SubjectIdentifier, bool LogoutAtIdentityProvider);