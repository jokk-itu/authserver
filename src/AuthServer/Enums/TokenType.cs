using System.ComponentModel;
using AuthServer.Constants;

namespace AuthServer.Enums;
public enum TokenType
{
  [Description(TokenTypeConstants.RefreshToken)]
  RefreshToken,

  [Description(TokenTypeConstants.AccessToken)]
  ClientAccessToken,

  [Description(TokenTypeConstants.AccessToken)]
  GrantAccessToken,

  RegistrationToken
}