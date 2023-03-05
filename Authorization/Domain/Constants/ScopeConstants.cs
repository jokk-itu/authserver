﻿namespace Domain.Constants;
public static class ScopeConstants
{
  public const string OfflineAccess = "offline_access";
  public const string Profile = "profile";
  public const string Email = "email";
  public const string Phone = "phone";
  public const string OpenId = "openid";
  public const string ClientRegistration = "identityprovider:clientregistration";
  public const string ClientConfiguration = "identityprovider:clientconfiguration";
  public const string ResourceRegistration = "identityprovider:resourceregistration";
  public const string ResourceConfiguration = "identityprovider:resourceconfiguration";
  public const string ScopeRegistration = "identityprovider:scoperegistration";
  public const string ScopeConfiguration = "identityprovider:scopeconfiguration";
  public const string UserInfo = "identityprovider:userinfo";

  public static readonly string[] PiiScopes = new[] { Profile, Email, Phone };
}