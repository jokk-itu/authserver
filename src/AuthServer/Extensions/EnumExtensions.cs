using System.ComponentModel;

namespace AuthServer.Extensions;
public static class EnumExtensions
{
    public static string GetDescription(this Enum value)
    {
        var type = value.GetType();
        var name = Enum.GetName(type, value);

        var field = type.GetField(name!) ?? throw new InvalidEnumArgumentException();
        var attribute = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) as DescriptionAttribute;

        return attribute?.Description ?? throw new InvalidEnumArgumentException();
    }
}