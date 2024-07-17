namespace AuthServer.Authorize;

public record AuthenticatedUser(string SubjectIdentifier, IEnumerable<string> Amr);