﻿namespace AuthServer.Authentication.Abstractions;

public interface IAuthenticatedUserAccessor
{
    Task<AuthenticatedUser?> GetAuthenticatedUser();
    Task<int> CountAuthenticatedUsers();
}