using System.ComponentModel;
using AuthServer.Constants;

namespace AuthServer.Enums;
public enum SubjectType
{
  [Description(SubjectTypeConstants.Public)]
  Public,

  [Description(SubjectTypeConstants.Pairwise)]
  Pairwise
}