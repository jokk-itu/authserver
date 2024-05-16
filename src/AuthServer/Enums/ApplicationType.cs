using System.ComponentModel;
using AuthServer.Constants;

namespace AuthServer.Enums;
public enum ApplicationType
{
  [Description(ApplicationTypeConstants.Web)]
  Web,

  [Description(ApplicationTypeConstants.Native)]
  Native
}