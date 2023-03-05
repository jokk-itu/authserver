using Application.Validation;
using Infrastructure.Requests;
using MediatR;
using Xunit;

namespace Specs.Verification;
public class ValidatorVerification
{
  [Fact]
  public void RequireAllRequestsToHaveAValidator()
  {
    var requests = typeof(Response).Assembly
      .GetTypes()
      .Where(x => x.IsClass)
      .Where(x => x.GetInterface(typeof(IRequest<>).Name) is not null)
      .ToList();

    var validators = typeof(Response).Assembly
      .GetTypes()
      .Where(x => x.IsClass)
      .Where(x => x.GetInterface(typeof(IValidator<>).Name) is not null)
      .Select(x => x.GetInterface(typeof(IValidator<>).Name))
      .OfType<Type>()
      .ToList();

    Assert.True(requests.All(x => validators.Any(y => y.GenericTypeArguments[0] == x)));
  }
}
