using Microsoft.AspNetCore.Mvc.Testing;

namespace Specs.Controllers;
public class ScopeControllerTests : BaseIntegrationTest
{
  public ScopeControllerTests(WebApplicationFactory<Program> applicationFactory)
  : base(applicationFactory)
  {
  }
}
