using System.ComponentModel;
using Domain.Constants;

namespace Domain.Enums;
public enum SubjectType
{
  [Description(SubjectTypeConstants.Pairwise)]
  Pairwise,

  [Description(SubjectTypeConstants.Public)]
  Public
}