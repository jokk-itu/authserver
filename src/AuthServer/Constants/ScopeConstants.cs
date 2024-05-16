﻿namespace AuthServer.Constants;
public static class ScopeConstants
{
    public const string OfflineAccess = "offline_access";
    public const string OpenId = "openid";
    public const string ClientConfiguration = "authserver:clientconfiguration";
    public const string UserInfo = "authserver:userinfo";

    public const string Profile = "profile";
    public const string Email = "email";
    public const string Address = "address";
    public const string Phone = "phone";

    public static readonly string[] Scopes = [OfflineAccess, OpenId, ClientConfiguration, UserInfo];
}