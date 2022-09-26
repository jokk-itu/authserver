using System.ComponentModel;

namespace Domain.Extensions;
public static class EnumExtensions
{
  public static string GetDescription(this Enum value)
  {
    var type = value.GetType();
    var name = Enum.GetName(type, value);

    var field = type.GetField(name!);
    if (field is null)
      throw new InvalidEnumArgumentException();

    var attr = 
      Attribute.GetCustomAttribute(field, 
        typeof(DescriptionAttribute)) as DescriptionAttribute;

    return attr?.Description ?? throw new InvalidEnumArgumentException();
  }
}