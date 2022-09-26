using System.ComponentModel;
using Domain.Constants;

namespace Domain.Enums;
public enum GrantType
{
  [Description(GrantTypeConstants.RefreshToken)]
  RefreshToken,

  [Description(GrantTypeConstants.AuthorizationCode)]
  AuthorizationCode
}