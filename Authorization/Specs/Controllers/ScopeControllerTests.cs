using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Specs.Controllers;

[Collection("Integration")]
public class ScopeControllerTests : BaseIntegrationTest
{
  public ScopeControllerTests(WebApplicationFactory<Program> applicationFactory)
  : base(applicationFactory)
  {
  }
}
