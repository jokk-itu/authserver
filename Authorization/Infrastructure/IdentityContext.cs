using Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Infrastructure;

public class IdentityContext : IdentityDbContext<User, IdentityRole, string>
{

  public IdentityContext(DbContextOptions<IdentityContext> options) : base(options)
  { }

  protected override void OnModelCreating(ModelBuilder builder)
  {
    base.OnModelCreating(builder);
    builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
  }

  /*
  private void SetResources(ModelBuilder builder)
  {
    var resource = new Resource
    {
      Id = "api1"
    };
    builder.Entity<Resource>().HasData(resource);
    builder.Entity<ResourceScope>().HasData(
        new ResourceScope
        {
          ResourceId = "api1",
          ScopeId = "api1"
        });
  }

  private void SetScopes(ModelBuilder builder)
  {
    var openId = new IdentityScope
    {
      Id = "openid"
    };
    var profile = new IdentityScope
    {
      Id = "profile"
    };
    var api1 = new IdentityScope
    {
      Id = "api1"
    };
    builder.Entity<IdentityScope>().HasData(openId, profile, api1);
  }

  private void SetClients(ModelBuilder builder)
  {
    //Clients
    var client = new Client
    {
      Id = "test",
      SecretHash = "secret".Sha256(),
      ClientType = ClientType.Confidential,
      ClientProfile = ClientProfile.WebApplication
    };
    builder.Entity<Client>().HasData(client);

    //ClientGrants
    builder.Entity<ClientGrant<string>>().HasData(
        new ClientGrant<string>
        {
          ClientId = client.Id,
          Name = "authorization_code"
        },
        new ClientGrant<string>
        {
          ClientId = client.Id,
          Name = "refresh_token"
        },
        new ClientGrant<string>
        {
          ClientId = client.Id,
          Name = "device"
        });

    //ClientScopes
    builder.Entity<ClientScope<string>>().HasData(
        new ClientScope<string>
        {
          ClientId = client.Id,
          ScopeId = "profile"
        },
        new ClientScope<string>
        {
          ClientId = client.Id,
          ScopeId = "openid"
        },
        new ClientScope<string>
        {
          ClientId = client.Id,
          ScopeId = "api1"
        });

    //ClientRedirectUris
    builder.Entity<RedirectUri<string>>().HasData(
        new RedirectUri<string>
        {
          ClientId = client.Id,
          Uri = "http://localhost:5002/callback"
        });
  }

  private void SetRoles(ModelBuilder builder)
  {
    //Roles
    var admin = new IdentityRole("Admin");
    builder.Entity<IdentityRole>()
        .HasData(admin);
  }*/
}