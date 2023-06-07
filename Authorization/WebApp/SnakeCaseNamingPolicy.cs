using System.Text.Json;
using WebApp.Extensions;

namespace WebApp;

public class SnakeCaseNamingPolicy : JsonNamingPolicy
{
  public override string ConvertName(string name) => name.ToSnakeCase();
}