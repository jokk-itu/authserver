using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using WebApp.Constants;

namespace WebApp.Contracts.PostRegisterUser;

public record PostRegisterUserRequest
{
  [FromForm(Name = ParameterNames.Username)]
  public string Username { get; init; } = null!;

  [FromForm(Name = ParameterNames.Password)]
  [PasswordPropertyText]
  public string Password { get; init; } = null!;

  [FromForm(Name = ParameterNames.Email)]
  [EmailAddress]
  public string Email { get; init; } = null!;

  [FromForm(Name = ParameterNames.Phonenumber)]
  [Phone]
  public string PhoneNumber { get; init; } = null!;

  [FromForm(Name = ParameterNames.Address)]
  public string Address { get; init; } = null!;

  [FromForm(Name = ParameterNames.Lastname)]
  public string FamilyName { get; init; } = null!;

  [FromForm(Name = ParameterNames.Firstname)]
  public string GivenName { get; init; } = null!;

  [FromForm(Name = ParameterNames.Birthdate)]
  public DateTime BirthDate { get; init; }

  [FromForm(Name = ParameterNames.Locale)]
  public string Locale { get; init; } = null!;
}