﻿using System.Net;

namespace Infrastructure.Requests.Login;

#nullable disable
public class LoginResponse : Response
{
  public LoginResponse(HttpStatusCode statusCode) : base(statusCode)
  {
  }

  public LoginResponse(string errorCode, string errorDescription, HttpStatusCode statusCode)
      : base(errorCode, errorDescription, statusCode)
  {
  }

  public string UserId { get; init; }
  public string Name { get; init; }
}