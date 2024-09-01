namespace AuthServer.Core;

public record AuthenticatedUser(string SubjectIdentifier, IEnumerable<string> Amr);