using System.ComponentModel;
using System.Reflection;

namespace Domain.Extensions;
public static class EnumExtensions
{
  public static string? GetDescription(this Enum value)
  {
    var type = value.GetType();
    var name = Enum.GetName(type, value);
    if (name is null)
      return null;

    var field = type.GetField(name);
    if (field is null) 
      return null;

    var attr = 
      Attribute.GetCustomAttribute(field, 
        typeof(DescriptionAttribute)) as DescriptionAttribute;

    return attr?.Description;
  }
}
