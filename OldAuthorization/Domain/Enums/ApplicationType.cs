using System.ComponentModel;
using Domain.Constants;

namespace Domain.Enums;
public enum ApplicationType
{
  [Description(ApplicationTypeConstants.Web)]
  Web,

  [Description(ApplicationTypeConstants.Native)]
  Native
}