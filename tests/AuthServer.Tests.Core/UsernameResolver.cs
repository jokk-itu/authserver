﻿using AuthServer.Introspection;

namespace AuthServer.Tests.Core;
public class UsernameResolver : IUsernameResolver
{
    public Task<string?> GetUsername(string subjectIdentifier) => Task.FromResult<string?>(UserConstants.Username);
}