﻿// <auto-generated />
using System;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace Infrastructure.Migrations
{
    [DbContext(typeof(IdentityContext))]
    partial class IdentityContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.10")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder, 1L, 1);

            modelBuilder.Entity("ClaimConsentGrant", b =>
                {
                    b.Property<int>("ConsentGrantsId")
                        .HasColumnType("int");

                    b.Property<int>("ConsentedClaimsId")
                        .HasColumnType("int");

                    b.HasKey("ConsentGrantsId", "ConsentedClaimsId");

                    b.HasIndex("ConsentedClaimsId");

                    b.ToTable("ConsentedGrantClaims", (string)null);
                });

            modelBuilder.Entity("ClientContact", b =>
                {
                    b.Property<string>("ClientsId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<int>("ContactsId")
                        .HasColumnType("int");

                    b.HasKey("ClientsId", "ContactsId");

                    b.HasIndex("ContactsId");

                    b.ToTable("ClientContacts", (string)null);
                });

            modelBuilder.Entity("ClientGrantType", b =>
                {
                    b.Property<string>("ClientsId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<int>("GrantTypesId")
                        .HasColumnType("int");

                    b.HasKey("ClientsId", "GrantTypesId");

                    b.HasIndex("GrantTypesId");

                    b.ToTable("ClientGrantTypes", (string)null);
                });

            modelBuilder.Entity("ClientResponseType", b =>
                {
                    b.Property<string>("ClientsId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<int>("ResponseTypesId")
                        .HasColumnType("int");

                    b.HasKey("ClientsId", "ResponseTypesId");

                    b.HasIndex("ResponseTypesId");

                    b.ToTable("ClientResponseTypes", (string)null);
                });

            modelBuilder.Entity("ClientScope", b =>
                {
                    b.Property<string>("ClientsId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<int>("ScopesId")
                        .HasColumnType("int");

                    b.HasKey("ClientsId", "ScopesId");

                    b.HasIndex("ScopesId");

                    b.ToTable("ClientScopes", (string)null);
                });

            modelBuilder.Entity("ConsentGrantScope", b =>
                {
                    b.Property<int>("ConsentGrantsId")
                        .HasColumnType("int");

                    b.Property<int>("ConsentedScopesId")
                        .HasColumnType("int");

                    b.HasKey("ConsentGrantsId", "ConsentedScopesId");

                    b.HasIndex("ConsentedScopesId");

                    b.ToTable("ConsentedGrantScopes", (string)null);
                });

            modelBuilder.Entity("Domain.AuthorizationCode", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("AuthorizationCodeGrantId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<bool>("IsRedeemed")
                        .HasColumnType("bit");

                    b.Property<DateTime>("IssuedAt")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("RedeemedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("Value")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("AuthorizationCodeGrantId");

                    b.ToTable("AuthorizationCode", (string)null);
                });

            modelBuilder.Entity("Domain.AuthorizationCodeGrant", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<DateTime>("AuthTime")
                        .HasColumnType("datetime2");

                    b.Property<string>("ClientId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<bool>("IsRevoked")
                        .HasColumnType("bit");

                    b.Property<long?>("MaxAge")
                        .HasColumnType("bigint");

                    b.Property<string>("SessionId")
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Id");

                    b.HasIndex("ClientId");

                    b.HasIndex("SessionId");

                    b.ToTable("AuthorizationCodeGrants", (string)null);
                });

            modelBuilder.Entity("Domain.Claim", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Claims", (string)null);

                    b.HasData(
                        new
                        {
                            Id = 1,
                            Name = "name"
                        },
                        new
                        {
                            Id = 2,
                            Name = "given_name"
                        },
                        new
                        {
                            Id = 3,
                            Name = "family_name"
                        },
                        new
                        {
                            Id = 4,
                            Name = "phone"
                        },
                        new
                        {
                            Id = 5,
                            Name = "email"
                        },
                        new
                        {
                            Id = 6,
                            Name = "address"
                        },
                        new
                        {
                            Id = 7,
                            Name = "birthdate"
                        },
                        new
                        {
                            Id = 8,
                            Name = "locale"
                        },
                        new
                        {
                            Id = 9,
                            Name = "role"
                        });
                });

            modelBuilder.Entity("Domain.Client", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("ApplicationType")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ClientUri")
                        .HasColumnType("nvarchar(max)");

                    b.Property<long?>("DefaultMaxAge")
                        .HasColumnType("bigint");

                    b.Property<string>("InitiateLoginUri")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("LogoUri")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PolicyUri")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Secret")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("SubjectType")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("TokenEndpointAuthMethod")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("TosUri")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Clients", (string)null);
                });

            modelBuilder.Entity("Domain.ConsentGrant", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<string>("ClientId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<DateTime>("Updated")
                        .HasColumnType("datetime2");

                    b.Property<string>("UserId")
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Id");

                    b.HasIndex("ClientId");

                    b.HasIndex("UserId");

                    b.ToTable("ConsentGrants", (string)null);
                });

            modelBuilder.Entity("Domain.Contact", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<string>("Email")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Contacts", (string)null);
                });

            modelBuilder.Entity("Domain.GrantType", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("GrantTypes", (string)null);

                    b.HasData(
                        new
                        {
                            Id = 1,
                            Name = "authorization_code"
                        },
                        new
                        {
                            Id = 2,
                            Name = "refresh_token"
                        },
                        new
                        {
                            Id = 3,
                            Name = "client_credentials"
                        });
                });

            modelBuilder.Entity("Domain.Jwk", b =>
                {
                    b.Property<long>("KeyId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<long>("KeyId"), 1L, 1);

                    b.Property<DateTime>("CreatedTimestamp")
                        .HasColumnType("datetime2");

                    b.Property<byte[]>("Exponent")
                        .HasColumnType("varbinary(max)");

                    b.Property<byte[]>("Modulus")
                        .HasColumnType("varbinary(max)");

                    b.Property<byte[]>("PrivateKey")
                        .HasColumnType("varbinary(max)");

                    b.HasKey("KeyId");

                    b.ToTable("Jwks", (string)null);
                });

            modelBuilder.Entity("Domain.Nonce", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("AuthorizationCodeGrantId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Value")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("AuthorizationCodeGrantId");

                    b.ToTable("Nonce", (string)null);
                });

            modelBuilder.Entity("Domain.RedirectUri", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<string>("ClientId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<int>("Type")
                        .HasColumnType("int");

                    b.Property<string>("Uri")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("ClientId");

                    b.ToTable("RedirectUris", (string)null);
                });

            modelBuilder.Entity("Domain.Resource", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Secret")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Resources", (string)null);
                });

            modelBuilder.Entity("Domain.ResponseType", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("ResponseTypes", (string)null);

                    b.HasData(
                        new
                        {
                            Id = 1,
                            Name = "code"
                        });
                });

            modelBuilder.Entity("Domain.Scope", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Id");

                    b.HasIndex("Name")
                        .IsUnique()
                        .HasFilter("[Name] IS NOT NULL");

                    b.ToTable("Scopes", (string)null);

                    b.HasData(
                        new
                        {
                            Id = 1,
                            Name = "openid"
                        },
                        new
                        {
                            Id = 2,
                            Name = "email"
                        },
                        new
                        {
                            Id = 3,
                            Name = "profile"
                        },
                        new
                        {
                            Id = 4,
                            Name = "offline_access"
                        },
                        new
                        {
                            Id = 5,
                            Name = "phone"
                        });
                });

            modelBuilder.Entity("Domain.Session", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<bool>("IsRevoked")
                        .HasColumnType("bit");

                    b.Property<string>("UserId")
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("Sessions", (string)null);
                });

            modelBuilder.Entity("Domain.User", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Address")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("Birthdate")
                        .HasColumnType("datetime2");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("IsEmailVerified")
                        .HasColumnType("bit");

                    b.Property<bool>("IsPhoneNumberVerified")
                        .HasColumnType("bit");

                    b.Property<string>("LastName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Locale")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PhoneNumber")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("UserName")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Id");

                    b.HasIndex("UserName")
                        .IsUnique();

                    b.ToTable("Users", (string)null);
                });

            modelBuilder.Entity("ResourceScope", b =>
                {
                    b.Property<string>("ResourcesId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<int>("ScopesId")
                        .HasColumnType("int");

                    b.HasKey("ResourcesId", "ScopesId");

                    b.HasIndex("ScopesId");

                    b.ToTable("ResourceScopes", (string)null);
                });

            modelBuilder.Entity("ClaimConsentGrant", b =>
                {
                    b.HasOne("Domain.ConsentGrant", null)
                        .WithMany()
                        .HasForeignKey("ConsentGrantsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Domain.Claim", null)
                        .WithMany()
                        .HasForeignKey("ConsentedClaimsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("ClientContact", b =>
                {
                    b.HasOne("Domain.Client", null)
                        .WithMany()
                        .HasForeignKey("ClientsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Domain.Contact", null)
                        .WithMany()
                        .HasForeignKey("ContactsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("ClientGrantType", b =>
                {
                    b.HasOne("Domain.Client", null)
                        .WithMany()
                        .HasForeignKey("ClientsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Domain.GrantType", null)
                        .WithMany()
                        .HasForeignKey("GrantTypesId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("ClientResponseType", b =>
                {
                    b.HasOne("Domain.Client", null)
                        .WithMany()
                        .HasForeignKey("ClientsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Domain.ResponseType", null)
                        .WithMany()
                        .HasForeignKey("ResponseTypesId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("ClientScope", b =>
                {
                    b.HasOne("Domain.Client", null)
                        .WithMany()
                        .HasForeignKey("ClientsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Domain.Scope", null)
                        .WithMany()
                        .HasForeignKey("ScopesId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("ConsentGrantScope", b =>
                {
                    b.HasOne("Domain.ConsentGrant", null)
                        .WithMany()
                        .HasForeignKey("ConsentGrantsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Domain.Scope", null)
                        .WithMany()
                        .HasForeignKey("ConsentedScopesId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Domain.AuthorizationCode", b =>
                {
                    b.HasOne("Domain.AuthorizationCodeGrant", "AuthorizationCodeGrant")
                        .WithMany("AuthorizationCodes")
                        .HasForeignKey("AuthorizationCodeGrantId");

                    b.Navigation("AuthorizationCodeGrant");
                });

            modelBuilder.Entity("Domain.AuthorizationCodeGrant", b =>
                {
                    b.HasOne("Domain.Client", "Client")
                        .WithMany("AuthorizationCodeGrants")
                        .HasForeignKey("ClientId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Domain.Session", "Session")
                        .WithMany("AuthorizationCodeGrants")
                        .HasForeignKey("SessionId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.Navigation("Client");

                    b.Navigation("Session");
                });

            modelBuilder.Entity("Domain.ConsentGrant", b =>
                {
                    b.HasOne("Domain.Client", "Client")
                        .WithMany("ConsentGrants")
                        .HasForeignKey("ClientId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Domain.User", "User")
                        .WithMany("ConsentGrants")
                        .HasForeignKey("UserId");

                    b.Navigation("Client");

                    b.Navigation("User");
                });

            modelBuilder.Entity("Domain.Nonce", b =>
                {
                    b.HasOne("Domain.AuthorizationCodeGrant", "AuthorizationCodeGrant")
                        .WithMany("Nonces")
                        .HasForeignKey("AuthorizationCodeGrantId");

                    b.Navigation("AuthorizationCodeGrant");
                });

            modelBuilder.Entity("Domain.RedirectUri", b =>
                {
                    b.HasOne("Domain.Client", "Client")
                        .WithMany("RedirectUris")
                        .HasForeignKey("ClientId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.Navigation("Client");
                });

            modelBuilder.Entity("Domain.Session", b =>
                {
                    b.HasOne("Domain.User", "User")
                        .WithMany("Sessions")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.NoAction);

                    b.Navigation("User");
                });

            modelBuilder.Entity("ResourceScope", b =>
                {
                    b.HasOne("Domain.Resource", null)
                        .WithMany()
                        .HasForeignKey("ResourcesId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Domain.Scope", null)
                        .WithMany()
                        .HasForeignKey("ScopesId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Domain.AuthorizationCodeGrant", b =>
                {
                    b.Navigation("AuthorizationCodes");

                    b.Navigation("Nonces");
                });

            modelBuilder.Entity("Domain.Client", b =>
                {
                    b.Navigation("AuthorizationCodeGrants");

                    b.Navigation("ConsentGrants");

                    b.Navigation("RedirectUris");
                });

            modelBuilder.Entity("Domain.Session", b =>
                {
                    b.Navigation("AuthorizationCodeGrants");
                });

            modelBuilder.Entity("Domain.User", b =>
                {
                    b.Navigation("ConsentGrants");

                    b.Navigation("Sessions");
                });
#pragma warning restore 612, 618
        }
    }
}
