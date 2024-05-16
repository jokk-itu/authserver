using System.ComponentModel;
using AuthServer.Constants;

namespace AuthServer.Enums;
public enum TokenEndpointAuthMethod
{
  [Description(TokenEndpointAuthMethodConstants.ClientSecretPost)]
  ClientSecretPost,

  [Description(TokenEndpointAuthMethodConstants.ClientSecretBasic)]
  ClientSecretBasic,

  [Description(TokenEndpointAuthMethodConstants.None)]
  None,

  [Description(TokenEndpointAuthMethodConstants.PrivateKeyJwt)]
  PrivateKeyJwt
}