using System.ComponentModel;

namespace Domain.Extensions;
public static class StringExtensions
{
  public static T? GetEnum<T>(this string value) where T : Enum
  {
    var @enum = typeof(T);
    var fields = @enum.GetFields();
    foreach (var field in fields)
    {
      var attribute = Attribute.GetCustomAttribute(field, 
        typeof(DescriptionAttribute)) as DescriptionAttribute;
      if (attribute?.Description == value)
        return (T)Enum.Parse(@enum, field.Name);
    }

    return default;
  }
}