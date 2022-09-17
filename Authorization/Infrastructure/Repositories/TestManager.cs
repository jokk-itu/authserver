using Domain;
using Domain.Enums;
using Infrastructure.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;
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
    await AddClientsAsync();
  }

	private async Task AddClientsAsync()
	{
    var client = new Client 
    {
      ClientProfile = ClientProfile.WebApplication,
      ClientType = ClientType.Confidential,
      Grants = await _identityContext.Set<Grant>().ToListAsync(),
      RedirectUris = await _identityContext.Set<RedirectUri>().ToListAsync(),
      Scopes = await _identityContext.Set<Scope>().ToListAsync(),
      Name = "test",
      Secret = "secret"
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
      Name = "identity-provider"
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
    await _userManager.AddToRolesAsync(user, new string[] {"Default", "SuperUser", "Administrator"});
  }

	private async Task AddResourcesAsync()
	{
    var apiResource = new Resource
    {
      Name = "api1",
			SecretHash = "secret",
      Scopes = await _identityContext.Set<Scope>().ToListAsync()
    };
    var identityResource = new Resource 
    {
      Name = "identity-provider",
      SecretHash = "secret",
      Scopes = await _identityContext.Set<Scope>().ToListAsync()
    };
		await _identityContext.Set<Resource>().AddRangeAsync(apiResource, identityResource);
		await _identityContext.SaveChangesAsync();
  }
}
