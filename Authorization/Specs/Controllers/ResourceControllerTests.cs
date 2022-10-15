using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Specs.Controllers;

[Collection("Integration")]
public class ResourceControllerTests : BaseIntegrationTest
{
  public ResourceControllerTests(WebApplicationFactory<Program> applicationFactory)
  : base(applicationFactory)
  {
  }
}
