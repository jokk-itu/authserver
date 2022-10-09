using Domain;
using Domain.Constants;
using Domain.Enums;
using Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Bogus;

namespace Specs;
public class TestManager
{
	private readonly IdentityContext _identityContext;
	private readonly UserManager<User> _userManager;
  private readonly RoleManager<IdentityRole> _roleManager;

  public TestManager(
    IdentityContext identityContext, 
    UserManager<User> userManager, 
    RoleManager<IdentityRole> roleManager)
	{
		_identityContext = identityContext;
		_userManager = userManager;
    _roleManager = roleManager;
  }

  public async Task AddDataAsync()
  {
    await AddRedirectUrisAsync();
    await AddScopesAsync();
    await AddRolesAsync();
    await AddUsersAsync();
    await AddResourcesAsync();
    await AddContactsAsync();
    await AddClientsAsync();
  }

  private async Task AddContactsAsync()
  {
    var contact = new Contact
    {
      Email = "contact@contact.com"
    };
    await _identityContext.Set<Contact>().AddAsync(contact);
    await _identityContext.SaveChangesAsync();
  }

	private async Task AddClientsAsync()
	{
    var client = new Client 
    {
      Id = Guid.NewGuid().ToString(),
      ClientProfile = ClientProfile.WebApplication,
      ClientType = ClientType.Confidential,
      GrantTypes = await _identityContext.Set<GrantType>().ToListAsync(),
      RedirectUris = await _identityContext.Set<RedirectUri>().ToListAsync(),
      Scopes = await _identityContext.Set<Scope>().ToListAsync(),
      Name = "test",
      Secret = "secret",
      Contacts = await _identityContext.Set<Contact>().ToListAsync(),
      PolicyUri = "http://localhost:5002/policy",
      SubjectType = SubjectType.Public,
      TokenEndpointAuthMethod = TokenEndpointAuthMethod.ClientSecretPost,
      TosUri = "http://localhost:5002/tos",
      ResponseTypes = await _identityContext.Set<ResponseType>().Where(x => x.Name == ResponseTypeConstants.Code).ToListAsync(),
    };
    await _identityContext.Set<Client>().AddAsync(client);
    await _identityContext.SaveChangesAsync();
	}

  private async Task AddRedirectUrisAsync()
  {
    var redirectUri = new RedirectUri
    {
      Uri = "http://localhost:5002/callback"
    };
    await _identityContext.Set<RedirectUri>().AddAsync(redirectUri);
    await _identityContext.SaveChangesAsync();
  } 

  private async Task AddScopesAsync()
  {
    var apiScope = new Scope
    {
      Name = "api1"
    };
    var identityScope = new Scope 
    {
      Name = "identityprovider"
    };
    await _identityContext.Set<Scope>().AddRangeAsync(apiScope, identityScope);
    await _identityContext.SaveChangesAsync();
  }

  private async Task AddRolesAsync()
  {
    await _roleManager.CreateAsync(new IdentityRole 
    {
      Name = "SuperUser",
      NormalizedName = "SUPERUSER"
    });
    await _roleManager.CreateAsync(new IdentityRole
    {
      Name = "Administrator",
      NormalizedName = "ADMINISTRATOR"
    });
    await _roleManager.CreateAsync(new IdentityRole
    {
      Name = "Default",
      NormalizedName = "DEFAULT"
    });
  }

	private async Task AddUsersAsync()
	{
    var user = new User
    {
      Address = "John Doe Street, 51",
      Birthdate = DateTime.Now,
      Locale = "da-DK",
      LastName = "Doe",
      FirstName = "John",
      UserName = "jokk",
      NormalizedEmail = "HEJMEDDIG@GMAIL.COM",
      NormalizedUserName = "JOKK",
      Email = "hejmeddig@gmail.com",
      PhoneNumber = "88888888"
    };
    await _userManager.CreateAsync(user, "Password12!");
    await _userManager.AddToRolesAsync(user, new[] {"Default", "SuperUser", "Administrator"});
  }

	private async Task AddResourcesAsync()
	{
    var apiResource = new Resource
    {
      Id = Guid.NewGuid().ToString(),
      Name = "api1",
			Secret = "secret",
      Scopes = await _identityContext.Set<Scope>().ToListAsync()
    };
    var identityResource = new Resource 
    {
      Id = Guid.NewGuid().ToString(),
      Name = "identityprovider",
      Secret = "secret",
      Scopes = await _identityContext.Set<Scope>().ToListAsync()
    };
		await _identityContext
      .Set<Resource>()
      .AddRangeAsync(apiResource, identityResource);

		await _identityContext.SaveChangesAsync();
  }
}
