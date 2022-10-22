using Bogus;
using Domain;

namespace Specs;
public static class FakeBuilder
{
  public static readonly Faker<User> UserFaker = new Faker<User>()
    .RuleFor(u => u.FirstName, p => p.Name.FirstName())
    .RuleFor(u => u.LastName, p => p.Name.LastName())
    .RuleFor(u => u.Address, f => f.Address.StreetAddress())
    .RuleFor(u => u.Birthdate, f => f.Date.Past(Random.Shared.Next(70)))
    .RuleFor(u => u.Email, f => f.Person.Email)
    .RuleFor(u => u.PhoneNumber, f => f.Phone.PhoneNumber())
    .RuleFor(u => u.UserName, f => f.Person.UserName)
    .RuleFor(u => u.Locale, f => f.Locale);
}
