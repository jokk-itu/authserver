using System.Net;

namespace Infrastructure.Requests.CreateResource;
public class CreateResourceResponse : Response
{
  public CreateResourceResponse(HttpStatusCode statusCode) : base(statusCode)
  {
  }

  public CreateResourceResponse(string? errorCode, string? errorDescription, HttpStatusCode statusCode) : base(errorCode, errorDescription, statusCode)
  {
  }

  public string ResourceId { get; set; } = null!;

  public string ResourceName { get; set; } = null!;

  public string ResourceSecret { get; set; } = null!;

  public string ResourceRegistrationAccessToken { get; set; } = null!;

  public string Scope { get; set; } = null!;
}