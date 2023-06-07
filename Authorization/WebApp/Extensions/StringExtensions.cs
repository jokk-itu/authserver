namespace WebApp.Extensions;

public static class StringExtensions
{
  public static string ToSnakeCase(this string value)
  {
    return string.Concat(value.Select((x, i) => i > 0 && char.IsUpper(x) ? "_" + x : x.ToString())).ToLower();
  }
}
