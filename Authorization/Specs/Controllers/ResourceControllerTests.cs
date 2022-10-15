using Domain.Constants;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net;
using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;
using WebApp.Contracts.GetResourceInitialAccessToken;
using WebApp.Contracts.PostResource;

namespace Specs.Controllers;
public class ResourceControllerTests : BaseIntegrationTest
{
  public ResourceControllerTests(WebApplicationFactory<Program> applicationFactory)
  : base(applicationFactory)
  {
  }
}
