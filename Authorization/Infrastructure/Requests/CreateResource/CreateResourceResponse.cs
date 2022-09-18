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

  public string ResourceId { get; set; }

  public string ResourceName { get; set; }

  public string ResourceSecret { get; set; }

  public string ResourceRegistrationAccessToken { get; set; }

  public string Scope { get; set; }
}
