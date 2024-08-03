namespace AuthServer.Authorize;

public record AuthorizeUser(string SubjectIdentifier, IEnumerable<string> Amr);