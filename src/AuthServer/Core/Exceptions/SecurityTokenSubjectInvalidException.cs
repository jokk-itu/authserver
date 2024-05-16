using Microsoft.IdentityModel.Tokens;

namespace AuthServer.Core.Exceptions;

public class SecurityTokenSubjectInvalidException(string message) : SecurityTokenException(message);