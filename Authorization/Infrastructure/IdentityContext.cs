using AuthorizationServer.Entities;
using AuthorizationServer.Extensions;
using Domain;
using Domain.Constants;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AuthorizationServer;

public class IdentityContext : IdentityDbContext<IdentityUserExtended, IdentityRole, string>
{
  public DbSet<IdentityClient> Clients { get; set; }
  public DbSet<IdentityClientScope<string>> ClientScopes { get; set; }
  public DbSet<IdentityClientGrant<string>> ClientGrants { get; set; }
  public DbSet<IdentityClientRedirectUri<string>> ClientRedirectUris { get; set; }
  public DbSet<IdentityClientToken<string>> ClientTokens { get; set; }
  public DbSet<IdentityScope> Scopes { get; set; }
  public DbSet<IdentityResource> Resources { get; set; }
  public DbSet<IdentityResourceScope> ResourceScopes { get; set; }
  public DbSet<IdentityJwk> Jwks { get; set; }

  public IdentityContext(DbContextOptions<IdentityContext> options) : base(options)
  { }

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

    //Jwk
    builder.Entity<IdentityJwk>(b =>
    {
      b.HasKey(jwk => jwk.KeyId);
      b.ToTable("AspNetJwks");
    });

    SetScopes(builder);
    SetResources(builder);
    SetClients(builder);
    SetRoles(builder);
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
    var client = new IdentityClient
    {
      Id = "test",
      SecretHash = "secret".Sha256(),
      ClientType = ClientType.Confidential,
      ClientProfile = ClientProfile.WebApplication
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
        },
        new IdentityClientScope<string>
        {
          ClientId = client.Id,
          ScopeId = "api1"
        });

    //ClientRedirectUris
    builder.Entity<IdentityClientRedirectUri<string>>().HasData(
        new IdentityClientRedirectUri<string>
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
  }
}