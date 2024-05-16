﻿namespace AuthServer.TokenDecoders;
public class ClientIssuedTokenDecodeArguments
{
    public required bool ValidateLifetime { get; init; }
    public required string ClientId { get; init; }
    public string? SubjectId { get; init; }
    public required IReadOnlyCollection<string> TokenTypes { get; init; }
    public required ClientTokenAudience Audience { get; init; }
}