using System.ComponentModel;

namespace AuthServer.Extensions;
public static class StringExtensions
{
    /// <summary>
    /// Gets the Enum of type <see cref="T"/>,
    /// based off string matching on the <see cref="DescriptionAttribute"/>
    /// for the Enum values.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="value"></param>
    /// <returns></returns>
    public static T? GetEnum<T>(this string value) where T : Enum
    {
        var @enum = typeof(T);
        var fields = @enum.GetFields();
        foreach (var field in fields)
        {
            var attribute = Attribute.GetCustomAttribute(field,
                typeof(DescriptionAttribute)) as DescriptionAttribute;

            if (attribute?.Description == value)
            {
                return (T)Enum.Parse(@enum, field.Name);
            }
        }

        return default;
    }
}