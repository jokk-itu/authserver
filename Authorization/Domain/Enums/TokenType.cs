using System.ComponentModel;
using Domain.Constants;

namespace Domain.Enums;
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