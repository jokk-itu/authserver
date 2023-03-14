namespace WebApp.Context.Abstract;

public interface IContextAccessor<T> where T : class
{
    Task<T> GetContext(HttpContext httpContext);
}