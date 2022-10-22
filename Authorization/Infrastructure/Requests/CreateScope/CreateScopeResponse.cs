using System.Net;
using Application.Validation;

namespace Infrastructure.Requests.CreateScope;
public class CreateScopeResponse : ValidationResult
{
  public CreateScopeResponse(HttpStatusCode statusCode) : base(statusCode)
  {
  }

  public CreateScopeResponse(string? errorCode, string? errorDescription, HttpStatusCode statusCode) : base(errorCode, errorDescription, statusCode)
  {
  }

  public int Id { get; init; }

  public string ScopeName { get; init; } = null!;

  public string ScopeRegistrationAccessToken { get; set; } = null!;
}