using Bogus;
using Domain;

namespace Specs.Helpers.Builders;
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

  public UserBuilder AddSession(Session session)
  {
    _user.Session = session;
    return this;
  }

  public UserBuilder AddConsentGrant(ConsentGrant consentGrant)
  {
    _user.ConsentGrants.Add(consentGrant);
    return this;
  }
}
