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

	public TestManager(IdentityContext identityContext, UserManager<User> userManager)
	{
		_identityContext = identityContext;
		_userManager = userManager;
	}

  public async Task AddDataAsync()
  {
    await AddRedirectUrisAsync();
    await AddScopesAsync();
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
      SecretHash = "secret".Sha256()
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
    var scope = new Scope
    {
      Name = "api1"
    };
    await _identityContext.Set<Scope>().AddAsync(scope);
    await _identityContext.SaveChangesAsync();
  }

	private async Task AddUsersAsync()
	{
    await _userManager.CreateAsync(new User
    {
      Address = "John Doe Street, 51",
      Name = "John WaitForIt Doe",
      Birthdate = DateTime.Now,
      Gender = "Man",
      Locale = "DA-DK",
      FamilyName = "Doe",
      MiddleName = "WaitForIt",
      GivenName = "John",
      NickName = "John",
      UserName = "jokk",
      NormalizedEmail = "HEJMEDDIG@GMAIL.COM",
      NormalizedUserName = "JOKK",
      Email = "hejmeddig@gmail.com",
      PhoneNumber = "88888888"
    }, "Password12!");
  }

	private async Task AddResourcesAsync()
	{
    var resource = new Resource
    {
      Name = "api1",
			SecretHash = "secret".Sha256(),
      Scopes = await _identityContext.Set<Scope>().ToListAsync()
    };
		await _identityContext.Set<Resource>().AddAsync(resource);
		await _identityContext.SaveChangesAsync();
  }
}
