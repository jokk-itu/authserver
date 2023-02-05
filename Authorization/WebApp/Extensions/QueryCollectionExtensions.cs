namespace WebApp.Extensions;

public static class QueryCollectionExtensions
{
  public static RouteValueDictionary ToRouteValueDictionary(this IQueryCollection query)
  {
    var routeValues = new RouteValueDictionary();
    foreach (var (key, value) in query)
    {
      routeValues.Add(key, value);
    }

    return routeValues;
  }
}
