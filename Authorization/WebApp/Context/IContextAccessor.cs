namespace WebApp.Context;

public interface IContextAccessor<T> where T : class
{
  Task<T> GetContext(HttpContext httpContext);
}