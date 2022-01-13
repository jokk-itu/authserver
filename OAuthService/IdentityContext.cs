using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OAuthService.Entities;

namespace OAuthService;

public class IdentityContext : IdentityDbContext
{
    public DbSet<IdentityClient> Clients { get; set; }
    public DbSet<IdentityClientScope<string>> ClientScopes { get; set; }
    public DbSet<IdentityClientGrant<string>> ClientGrants { get; set; }
    public DbSet<IdentityClientRedirectUri<string>> ClientRedirectUris { get; set; }

    public DbSet<IdentityClientToken<string>> ClientTokens { get; set; }

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
            b.HasKey(cs => new { cs.Name, cs.ClientId });
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
            //TODO place constraint on URI, and demand HTTPS
        });
        
        //ClientTokens
        builder.Entity<IdentityClientToken<string>>(b =>
        {
            b.HasKey(ct => new { ct.ClientId, ct.Value });
            b.ToTable("AspNetClientTokens");
        });

        SetClient(builder);
        SetUser(builder);
    }

    private void SetClient(ModelBuilder builder)
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
                Name = "password"
            },
            new IdentityClientGrant<string>
            {
                ClientId = client.Id,
                Name = "client_credentials"
            });

        //ClientScopes
        builder.Entity<IdentityClientScope<string>>().HasData(
            new IdentityClientScope<string>
            {
                ClientId = client.Id,
                Name = "profile"
            },
            new IdentityClientScope<string>
            {
                ClientId = client.Id,
                Name = "openid"
            });

        //ClientRedirectUris
        builder.Entity<IdentityClientRedirectUri<string>>().HasData(
            new IdentityClientRedirectUri<string>
            {
                ClientId = client.Id,
                Uri = "http://localhost:5002/callback"
            });
    }

    private void SetUser(ModelBuilder builder)
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
                    Id = 3,
                    ClaimType = ClaimTypes.Surname,
                    ClaimValue = "Kelsen",
                    UserId = jokk.Id
                },
                new IdentityUserClaim<string>
                {
                    Id = 2,
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