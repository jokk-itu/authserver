using Infrastructure.Requests;
using MediatR;
using Xunit;

namespace Specs.Verification;
public class RequestHandlerVerification
{
  [Fact]
  public void RequireAllRequestsToHaveARequestsHandler()
  {
    var requests = typeof(Response).Assembly
      .GetTypes()
      .Where(x => x.IsClass)
      .Where(x => x.GetInterface(typeof(IRequest<>).Name) is not null)
      .ToList();

    var requestHandlers = typeof(Response).Assembly
      .GetTypes()
      .Where(x => x.IsClass)
      .Where(x => x.GetInterface(typeof(IRequestHandler<,>).Name) is not null)
      .Select(x => x.GetInterface(typeof(IRequestHandler<,>).Name))
      .OfType<Type>()
      .ToList();

    Assert.True(requests.All(x => requestHandlers.Any(y => y.GenericTypeArguments[0] == x)));
  }
}