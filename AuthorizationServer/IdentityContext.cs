using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using AuthorizationServer.Entities;
using AuthorizationServer.Extensions;

namespace AuthorizationServer;

public class IdentityContext : IdentityDbContext
{
  public DbSet<IdentityClient> Clients { get; set; }
  public DbSet<IdentityClientScope<string>> ClientScopes { get; set; }
  public DbSet<IdentityClientGrant<string>> ClientGrants { get; set; }
  public DbSet<IdentityClientRedirectUri<string>> ClientRedirectUris { get; set; }
  public DbSet<IdentityClientToken<string>> ClientTokens { get; set; }

  public DbSet<IdentityScope> Scopes { get; set; }

  public DbSet<IdentityResource> Resources { get; set; }

  public DbSet<IdentityResourceScope> ResourceScopes { get; set; }

  public DbSet<RSAKeyPair> KeyPairs { get; set; }

  public IdentityContext(DbContextOptions<IdentityContext> options) : base(options)
  {
  }

  protected override void OnModelCreating(ModelBuilder builder)
  {
    base.OnModelCreating(builder);

    //Clients
    builder.Entity<IdentityClient>(b =>
    {
      b.HasKey(c => c.Id);
      b.Property(c => c.ConcurrencyStamp).IsConcurrencyToken();
      b.ToTable("AspNetClients");
    });

    //ClientScopes
    builder.Entity<IdentityClientScope<string>>(b =>
    {
      b.HasKey(cs => new { Name = cs.ScopeId, cs.ClientId });
      b.ToTable("AspNetClientScopes");
    });

    //ClientGrants
    builder.Entity<IdentityClientGrant<string>>(b =>
    {
      b.HasKey(cg => new { cg.Name, cg.ClientId });
      b.ToTable("AspNetClientGrants");
    });

    //ClientRedirectURI's
    builder.Entity<IdentityClientRedirectUri<string>>(b =>
    {
      b.HasKey(cru => new { cru.Uri, cru.ClientId });
      b.ToTable("AspNetClientRedirectUris");
    });

    //ClientTokens
    builder.Entity<IdentityClientToken<string>>(b =>
    {
      b.HasKey(ct => new { ct.ClientId, ct.Value });
      b.ToTable("AspNetClientTokens");
    });

    //Scopes
    builder.Entity<IdentityScope>(b =>
    {
      b.HasKey(s => s.Id);
      b.Property(s => s.ConcurrencyStamp).IsConcurrencyToken();
      b.ToTable("AspNetScopes");
    });

    //Resources
    builder.Entity<IdentityResource>(b =>
    {
      b.HasKey(r => r.Id);
      b.Property(r => r.ConcurrencyStamp).IsConcurrencyToken();
      b.ToTable("AspNetResources");
    });

    //ResourceScopes
    builder.Entity<IdentityResourceScope>(b =>
    {
      b.HasKey(rs => new { rs.ResourceId, rs.ScopeId });
      b.ToTable("AspNetResourceScopes");
    });

    //PublicKey
    builder.Entity<RSAKeyPair>(b => 
    {
      b.HasKey(pb => new { pb.PublicKey, pb.PrivateKey });
      b.ToTable("KeyPairs");
    });

    SetScopes(builder);
    SetResources(builder);
    SetClients(builder);
    SetUsers(builder);
  }

  private void SetResources(ModelBuilder builder)
  {
    var resource = new IdentityResource
    {
      Id = "api1"
    };
    builder.Entity<IdentityResource>().HasData(resource);
    builder.Entity<IdentityResourceScope>().HasData(
        new IdentityResourceScope
        {
          ResourceId = "api1",
          ScopeId = "openid"
        },
        new IdentityResourceScope
        {
          ResourceId = "api1",
          ScopeId = "profile"
        },
        new IdentityResourceScope
        {
          ResourceId = "api1",
          ScopeId = "api1:read"
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
      Id = "api1:read"
    };
    builder.Entity<IdentityScope>().HasData(openId, profile, api1);
  }

  private void SetClients(ModelBuilder builder)
  {
    //Clients
    var client = new IdentityClient
    {
      Id = "test",
      SecretHash = "secret".Sha256(),
      ClientType = "confidential", //Or public
      ClientProfile = "web application" //or user-agent based application or native application
    };
    builder.Entity<IdentityClient>().HasData(client);

    //ClientGrants
    builder.Entity<IdentityClientGrant<string>>().HasData(
        new IdentityClientGrant<string>
        {
          ClientId = client.Id,
          Name = "authorization_code"
        },
        new IdentityClientGrant<string>
        {
          ClientId = client.Id,
          Name = "refresh_token"
        },
        new IdentityClientGrant<string>
        {
          ClientId = client.Id,
          Name = "device"
        },
        new IdentityClientGrant<string>
        {
          ClientId = client.Id,
          Name = "openid"
        });

    //ClientScopes
    builder.Entity<IdentityClientScope<string>>().HasData(
        new IdentityClientScope<string>
        {
          ClientId = client.Id,
          ScopeId = "profile"
        },
        new IdentityClientScope<string>
        {
          ClientId = client.Id,
          ScopeId = "openid"
        });

    //ClientRedirectUris
    builder.Entity<IdentityClientRedirectUri<string>>().HasData(
        new IdentityClientRedirectUri<string>
        {
          ClientId = client.Id,
          Uri = "http://localhost:5002/callback"
        });
  }

  private void SetUsers(ModelBuilder builder)
  {
    //Users
    var jokk = new IdentityUser
    {
      Email = "joachim@kelsen.nu",
      EmailConfirmed = true,
      NormalizedEmail = "JOACHIM@KELSEN.NU",
      UserName = "jokk",
      NormalizedUserName = "JOKK"
    };
    builder.Entity<IdentityUser>()
        .HasData(jokk);

    //Roles
    var admin = new IdentityRole("Admin");
    builder.Entity<IdentityRole>()
        .HasData(admin);

    //UserClaims
    builder.Entity<IdentityUserClaim<string>>()
        .HasData(
            new IdentityUserClaim<string>
            {
              Id = 1,
              ClaimType = ClaimTypes.Name,
              ClaimValue = "Joachim",
              UserId = jokk.Id
            },
            new IdentityUserClaim<string>
            {
              Id = 2,
              ClaimType = ClaimTypes.Surname,
              ClaimValue = "Kelsen",
              UserId = jokk.Id
            },
            new IdentityUserClaim<string>
            {
              Id = 3,
              ClaimType = ClaimTypes.Country,
              ClaimValue = "Denmark",
              UserId = jokk.Id
            });

    //UserRoles
    builder.Entity<IdentityUserRole<string>>().HasData(
        new IdentityUserRole<string>
        {
          RoleId = admin.Id,
          UserId = jokk.Id
        });
  }
}