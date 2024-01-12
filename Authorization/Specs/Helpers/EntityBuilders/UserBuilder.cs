using Bogus;
using Domain;
using Infrastructure.Helpers;

namespace Specs.Helpers.EntityBuilders;
public class UserBuilder
{
  private readonly User _user;

  private UserBuilder()
  {
    var faker = new Faker();
    _user = new User
    {
      Id = Guid.NewGuid().ToString(),
      Address = faker.Person.Address.Street,
      Birthdate = faker.Person.DateOfBirth,
      Email = faker.Person.Email,
      FirstName = faker.Person.FirstName,
      LastName = faker.Person.LastName,
      Locale = faker.Locale,
      UserName = faker.Person.UserName,
      PhoneNumber = faker.Person.Phone
    };
  }

  public static UserBuilder Instance()
  {
    return new UserBuilder();
  }

  public User Build()
  {
    return _user;
  }

  public UserBuilder AddPassword(string password)
  {
    var salt = BCrypt.GenerateSalt();
    var hashedPassword = BCrypt.HashPassword(password, salt);
    _user.Password = hashedPassword;
    return this;
  }

  public UserBuilder AddRole(Role role)
  {
    _user.Roles.Add(role);
    return this;
  }

  public UserBuilder AddSession(Session session)
  {
    _user.Sessions.Add(session);
    return this;
  }

  public UserBuilder AddConsentGrant(ConsentGrant consentGrant)
  {
    _user.ConsentGrants.Add(consentGrant);
    return this;
  }
}
