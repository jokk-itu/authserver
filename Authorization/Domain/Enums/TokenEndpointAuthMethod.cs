using System.ComponentModel;
using Domain.Constants;

namespace Domain.Enums;
public enum TokenEndpointAuthMethod
{
  [Description(TokenEndpointAuthMethodConstants.ClientSecretPost)]
  ClientSecretPost,

  [Description(TokenEndpointAuthMethodConstants.ClientSecretBasic)]
  ClientSecretBasic
}