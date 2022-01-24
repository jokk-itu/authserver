﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OAuthService.Migrations
{
  public partial class InitialCreate : Migration
  {
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.CreateTable(
          name: "AspNetClientGrants",
          columns: table => new
          {
            Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
            ClientId = table.Column<string>(type: "nvarchar(450)", nullable: false)
          },
          constraints: table =>
          {
            table.PrimaryKey("PK_AspNetClientGrants", x => new { x.Name, x.ClientId });
          });

      migrationBuilder.CreateTable(
          name: "AspNetClientRedirectUris",
          columns: table => new
          {
            Uri = table.Column<string>(type: "nvarchar(450)", nullable: false),
            ClientId = table.Column<string>(type: "nvarchar(450)", nullable: false)
          },
          constraints: table =>
          {
            table.PrimaryKey("PK_AspNetClientRedirectUris", x => new { x.Uri, x.ClientId });
          });

      migrationBuilder.CreateTable(
          name: "AspNetClients",
          columns: table => new
          {
            Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
            SecretHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
            ClientType = table.Column<string>(type: "nvarchar(max)", nullable: false),
            ClientProfile = table.Column<string>(type: "nvarchar(max)", nullable: false),
            ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: false)
          },
          constraints: table =>
          {
            table.PrimaryKey("PK_AspNetClients", x => x.Id);
          });

      migrationBuilder.CreateTable(
          name: "AspNetClientScopes",
          columns: table => new
          {
            Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
            ClientId = table.Column<string>(type: "nvarchar(450)", nullable: false)
          },
          constraints: table =>
          {
            table.PrimaryKey("PK_AspNetClientScopes", x => new { x.Name, x.ClientId });
          });

      migrationBuilder.CreateTable(
          name: "AspNetClientTokens",
          columns: table => new
          {
            ClientId = table.Column<string>(type: "nvarchar(450)", nullable: false),
            Value = table.Column<string>(type: "nvarchar(450)", nullable: false),
            Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
          },
          constraints: table =>
          {
            table.PrimaryKey("PK_AspNetClientTokens", x => new { x.ClientId, x.Value });
          });

      migrationBuilder.CreateTable(
          name: "AspNetRoles",
          columns: table => new
          {
            Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
            Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
            NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
            ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
          },
          constraints: table =>
          {
            table.PrimaryKey("PK_AspNetRoles", x => x.Id);
          });

      migrationBuilder.CreateTable(
          name: "AspNetUsers",
          columns: table => new
          {
            Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
            UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
            NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
            Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
            NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
            EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
            PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
            SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
            ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
            PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
            PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
            TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
            LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
            LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
            AccessFailedCount = table.Column<int>(type: "int", nullable: false)
          },
          constraints: table =>
          {
            table.PrimaryKey("PK_AspNetUsers", x => x.Id);
          });

      migrationBuilder.CreateTable(
          name: "AspNetRoleClaims",
          columns: table => new
          {
            Id = table.Column<int>(type: "int", nullable: false)
                  .Annotation("SqlServer:Identity", "1, 1"),
            RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
            ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
            ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
          },
          constraints: table =>
          {
            table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
            table.ForeignKey(
                      name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                      column: x => x.RoleId,
                      principalTable: "AspNetRoles",
                      principalColumn: "Id",
                      onDelete: ReferentialAction.Cascade);
          });

      migrationBuilder.CreateTable(
          name: "AspNetUserClaims",
          columns: table => new
          {
            Id = table.Column<int>(type: "int", nullable: false)
                  .Annotation("SqlServer:Identity", "1, 1"),
            UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
            ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
            ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
          },
          constraints: table =>
          {
            table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
            table.ForeignKey(
                      name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                      column: x => x.UserId,
                      principalTable: "AspNetUsers",
                      principalColumn: "Id",
                      onDelete: ReferentialAction.Cascade);
          });

      migrationBuilder.CreateTable(
          name: "AspNetUserLogins",
          columns: table => new
          {
            LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
            ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
            ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
            UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
          },
          constraints: table =>
          {
            table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
            table.ForeignKey(
                      name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                      column: x => x.UserId,
                      principalTable: "AspNetUsers",
                      principalColumn: "Id",
                      onDelete: ReferentialAction.Cascade);
          });

      migrationBuilder.CreateTable(
          name: "AspNetUserRoles",
          columns: table => new
          {
            UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
            RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
          },
          constraints: table =>
          {
            table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
            table.ForeignKey(
                      name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                      column: x => x.RoleId,
                      principalTable: "AspNetRoles",
                      principalColumn: "Id",
                      onDelete: ReferentialAction.Cascade);
            table.ForeignKey(
                      name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                      column: x => x.UserId,
                      principalTable: "AspNetUsers",
                      principalColumn: "Id",
                      onDelete: ReferentialAction.Cascade);
          });

      migrationBuilder.CreateTable(
          name: "AspNetUserTokens",
          columns: table => new
          {
            UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
            LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
            Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
            Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
          },
          constraints: table =>
          {
            table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
            table.ForeignKey(
                      name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                      column: x => x.UserId,
                      principalTable: "AspNetUsers",
                      principalColumn: "Id",
                      onDelete: ReferentialAction.Cascade);
          });

      migrationBuilder.InsertData(
          table: "AspNetClientGrants",
          columns: new[] { "ClientId", "Name" },
          values: new object[,]
          {
                    { "test", "authorization_code" },
                    { "test", "client_credentials" },
                    { "test", "password" },
                    { "test", "refresh_token" }
          });

      migrationBuilder.InsertData(
          table: "AspNetClientRedirectUris",
          columns: new[] { "ClientId", "Uri" },
          values: new object[] { "test", "http://localhost:5002/callback" });

      migrationBuilder.InsertData(
          table: "AspNetClientScopes",
          columns: new[] { "ClientId", "Name" },
          values: new object[,]
          {
                    { "test", "openid" },
                    { "test", "profile" }
          });

      migrationBuilder.InsertData(
          table: "AspNetClients",
          columns: new[] { "Id", "ClientProfile", "ClientType", "ConcurrencyStamp", "SecretHash" },
          values: new object[] { "test", "web application", "confidential", "1f1193ec-a87c-47ac-acdb-d6e1626c4463", "2BB80D537B1DA3E38BD30361AA855686BDE0EACD7162FEF6A25FE97BF527A25B" });

      migrationBuilder.InsertData(
          table: "AspNetRoles",
          columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
          values: new object[] { "262f60c9-c67d-45b7-8b99-600776025d73", "264d883e-017f-4c53-9ba0-7598b64a92b1", "Admin", null });

      migrationBuilder.InsertData(
          table: "AspNetUsers",
          columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "Email", "EmailConfirmed", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "TwoFactorEnabled", "UserName" },
          values: new object[] { "c90aab72-2f87-40a2-92ea-e505207069f0", 0, "358467d0-4e20-4867-804a-80cccb9ba0ff", "joachim@kelsen.nu", true, false, null, "JOACHIM@KELSEN.NU", "JOKK", null, null, false, "09e7a660-f9cd-4ff4-a6f6-12f01e6e4a51", false, "jokk" });

      migrationBuilder.InsertData(
          table: "AspNetUserClaims",
          columns: new[] { "Id", "ClaimType", "ClaimValue", "UserId" },
          values: new object[,]
          {
                    { 1, "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name", "Joachim", "c90aab72-2f87-40a2-92ea-e505207069f0" },
                    { 2, "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/country", "Denmark", "c90aab72-2f87-40a2-92ea-e505207069f0" },
                    { 3, "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname", "Kelsen", "c90aab72-2f87-40a2-92ea-e505207069f0" }
          });

      migrationBuilder.InsertData(
          table: "AspNetUserRoles",
          columns: new[] { "RoleId", "UserId" },
          values: new object[] { "262f60c9-c67d-45b7-8b99-600776025d73", "c90aab72-2f87-40a2-92ea-e505207069f0" });

      migrationBuilder.CreateIndex(
          name: "IX_AspNetRoleClaims_RoleId",
          table: "AspNetRoleClaims",
          column: "RoleId");

      migrationBuilder.CreateIndex(
          name: "RoleNameIndex",
          table: "AspNetRoles",
          column: "NormalizedName",
          unique: true,
          filter: "[NormalizedName] IS NOT NULL");

      migrationBuilder.CreateIndex(
          name: "IX_AspNetUserClaims_UserId",
          table: "AspNetUserClaims",
          column: "UserId");

      migrationBuilder.CreateIndex(
          name: "IX_AspNetUserLogins_UserId",
          table: "AspNetUserLogins",
          column: "UserId");

      migrationBuilder.CreateIndex(
          name: "IX_AspNetUserRoles_RoleId",
          table: "AspNetUserRoles",
          column: "RoleId");

      migrationBuilder.CreateIndex(
          name: "EmailIndex",
          table: "AspNetUsers",
          column: "NormalizedEmail");

      migrationBuilder.CreateIndex(
          name: "UserNameIndex",
          table: "AspNetUsers",
          column: "NormalizedUserName",
          unique: true,
          filter: "[NormalizedUserName] IS NOT NULL");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.DropTable(
          name: "AspNetClientGrants");

      migrationBuilder.DropTable(
          name: "AspNetClientRedirectUris");

      migrationBuilder.DropTable(
          name: "AspNetClients");

      migrationBuilder.DropTable(
          name: "AspNetClientScopes");

      migrationBuilder.DropTable(
          name: "AspNetClientTokens");

      migrationBuilder.DropTable(
          name: "AspNetRoleClaims");

      migrationBuilder.DropTable(
          name: "AspNetUserClaims");

      migrationBuilder.DropTable(
          name: "AspNetUserLogins");

      migrationBuilder.DropTable(
          name: "AspNetUserRoles");

      migrationBuilder.DropTable(
          name: "AspNetUserTokens");

      migrationBuilder.DropTable(
          name: "AspNetRoles");

      migrationBuilder.DropTable(
          name: "AspNetUsers");
    }
  }
}
