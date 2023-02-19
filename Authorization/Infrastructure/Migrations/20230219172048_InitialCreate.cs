using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Claims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Claims", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Clients",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Secret = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TosUri = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PolicyUri = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClientUri = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LogoUri = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InitiateLoginUri = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DefaultMaxAge = table.Column<long>(type: "bigint", nullable: true),
                    ApplicationType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TokenEndpointAuthMethod = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SubjectType = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clients", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Contacts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contacts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GrantTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GrantTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Jwks",
                columns: table => new
                {
                    KeyId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedTimestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PrivateKey = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    Modulus = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    Exponent = table.Column<byte[]>(type: "varbinary(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Jwks", x => x.KeyId);
                });

            migrationBuilder.CreateTable(
                name: "Resources",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Secret = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Resources", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ResponseTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResponseTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Scopes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Scopes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsEmailVerified = table.Column<bool>(type: "bit", nullable: false),
                    IsPhoneNumberVerified = table.Column<bool>(type: "bit", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Birthdate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Locale = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RedirectUris",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Uri = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClientId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RedirectUris", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RedirectUris_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ClientContacts",
                columns: table => new
                {
                    ClientsId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ContactsId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientContacts", x => new { x.ClientsId, x.ContactsId });
                    table.ForeignKey(
                        name: "FK_ClientContacts_Clients_ClientsId",
                        column: x => x.ClientsId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClientContacts_Contacts_ContactsId",
                        column: x => x.ContactsId,
                        principalTable: "Contacts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ClientGrantTypes",
                columns: table => new
                {
                    ClientsId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    GrantTypesId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientGrantTypes", x => new { x.ClientsId, x.GrantTypesId });
                    table.ForeignKey(
                        name: "FK_ClientGrantTypes_Clients_ClientsId",
                        column: x => x.ClientsId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClientGrantTypes_GrantTypes_GrantTypesId",
                        column: x => x.GrantTypesId,
                        principalTable: "GrantTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ClientResponseTypes",
                columns: table => new
                {
                    ClientsId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ResponseTypesId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientResponseTypes", x => new { x.ClientsId, x.ResponseTypesId });
                    table.ForeignKey(
                        name: "FK_ClientResponseTypes_Clients_ClientsId",
                        column: x => x.ClientsId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClientResponseTypes_ResponseTypes_ResponseTypesId",
                        column: x => x.ResponseTypesId,
                        principalTable: "ResponseTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ClientScopes",
                columns: table => new
                {
                    ClientsId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ScopesId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientScopes", x => new { x.ClientsId, x.ScopesId });
                    table.ForeignKey(
                        name: "FK_ClientScopes_Clients_ClientsId",
                        column: x => x.ClientsId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClientScopes_Scopes_ScopesId",
                        column: x => x.ScopesId,
                        principalTable: "Scopes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ResourceScopes",
                columns: table => new
                {
                    ResourcesId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ScopesId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResourceScopes", x => new { x.ResourcesId, x.ScopesId });
                    table.ForeignKey(
                        name: "FK_ResourceScopes_Resources_ResourcesId",
                        column: x => x.ResourcesId,
                        principalTable: "Resources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ResourceScopes_Scopes_ScopesId",
                        column: x => x.ScopesId,
                        principalTable: "Scopes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ConsentGrants",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Updated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ClientId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsentGrants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConsentGrants_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ConsentGrants_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Sessions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IsRevoked = table.Column<bool>(type: "bit", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Sessions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ConsentedGrantClaims",
                columns: table => new
                {
                    ConsentGrantsId = table.Column<int>(type: "int", nullable: false),
                    ConsentedClaimsId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsentedGrantClaims", x => new { x.ConsentGrantsId, x.ConsentedClaimsId });
                    table.ForeignKey(
                        name: "FK_ConsentedGrantClaims_Claims_ConsentedClaimsId",
                        column: x => x.ConsentedClaimsId,
                        principalTable: "Claims",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ConsentedGrantClaims_ConsentGrants_ConsentGrantsId",
                        column: x => x.ConsentGrantsId,
                        principalTable: "ConsentGrants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ConsentedGrantScopes",
                columns: table => new
                {
                    ConsentGrantsId = table.Column<int>(type: "int", nullable: false),
                    ConsentedScopesId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsentedGrantScopes", x => new { x.ConsentGrantsId, x.ConsentedScopesId });
                    table.ForeignKey(
                        name: "FK_ConsentedGrantScopes_ConsentGrants_ConsentGrantsId",
                        column: x => x.ConsentGrantsId,
                        principalTable: "ConsentGrants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ConsentedGrantScopes_Scopes_ConsentedScopesId",
                        column: x => x.ConsentedScopesId,
                        principalTable: "Scopes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AuthorizationCodeGrants",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Nonce = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsCodeRedeemed = table.Column<bool>(type: "bit", nullable: false),
                    AuthTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MaxAge = table.Column<long>(type: "bigint", nullable: true),
                    IsRevoked = table.Column<bool>(type: "bit", nullable: false),
                    SessionId = table.Column<int>(type: "int", nullable: true),
                    ClientId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthorizationCodeGrants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuthorizationCodeGrants_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AuthorizationCodeGrants_Sessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "Sessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Claims",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "name" },
                    { 2, "given_name" },
                    { 3, "family_name" },
                    { 4, "phone" },
                    { 5, "email" },
                    { 6, "address" },
                    { 7, "birthdate" },
                    { 8, "locale" },
                    { 9, "role" }
                });

            migrationBuilder.InsertData(
                table: "GrantTypes",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "authorization_code" },
                    { 2, "refresh_token" },
                    { 3, "client_credentials" }
                });

            migrationBuilder.InsertData(
                table: "ResponseTypes",
                columns: new[] { "Id", "Name" },
                values: new object[] { 1, "code" });

            migrationBuilder.InsertData(
                table: "Scopes",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "openid" },
                    { 2, "email" },
                    { 3, "profile" },
                    { 4, "offline_access" },
                    { 5, "phone" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuthorizationCodeGrants_ClientId",
                table: "AuthorizationCodeGrants",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_AuthorizationCodeGrants_SessionId",
                table: "AuthorizationCodeGrants",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientContacts_ContactsId",
                table: "ClientContacts",
                column: "ContactsId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientGrantTypes_GrantTypesId",
                table: "ClientGrantTypes",
                column: "GrantTypesId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientResponseTypes_ResponseTypesId",
                table: "ClientResponseTypes",
                column: "ResponseTypesId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientScopes_ScopesId",
                table: "ClientScopes",
                column: "ScopesId");

            migrationBuilder.CreateIndex(
                name: "IX_ConsentedGrantClaims_ConsentedClaimsId",
                table: "ConsentedGrantClaims",
                column: "ConsentedClaimsId");

            migrationBuilder.CreateIndex(
                name: "IX_ConsentedGrantScopes_ConsentedScopesId",
                table: "ConsentedGrantScopes",
                column: "ConsentedScopesId");

            migrationBuilder.CreateIndex(
                name: "IX_ConsentGrants_ClientId",
                table: "ConsentGrants",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_ConsentGrants_UserId",
                table: "ConsentGrants",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_RedirectUris_ClientId",
                table: "RedirectUris",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_ResourceScopes_ScopesId",
                table: "ResourceScopes",
                column: "ScopesId");

            migrationBuilder.CreateIndex(
                name: "IX_Scopes_Name",
                table: "Scopes",
                column: "Name",
                unique: true,
                filter: "[Name] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_UserId",
                table: "Sessions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_UserName",
                table: "Users",
                column: "UserName",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuthorizationCodeGrants");

            migrationBuilder.DropTable(
                name: "ClientContacts");

            migrationBuilder.DropTable(
                name: "ClientGrantTypes");

            migrationBuilder.DropTable(
                name: "ClientResponseTypes");

            migrationBuilder.DropTable(
                name: "ClientScopes");

            migrationBuilder.DropTable(
                name: "ConsentedGrantClaims");

            migrationBuilder.DropTable(
                name: "ConsentedGrantScopes");

            migrationBuilder.DropTable(
                name: "Jwks");

            migrationBuilder.DropTable(
                name: "RedirectUris");

            migrationBuilder.DropTable(
                name: "ResourceScopes");

            migrationBuilder.DropTable(
                name: "Sessions");

            migrationBuilder.DropTable(
                name: "Contacts");

            migrationBuilder.DropTable(
                name: "GrantTypes");

            migrationBuilder.DropTable(
                name: "ResponseTypes");

            migrationBuilder.DropTable(
                name: "Claims");

            migrationBuilder.DropTable(
                name: "ConsentGrants");

            migrationBuilder.DropTable(
                name: "Resources");

            migrationBuilder.DropTable(
                name: "Scopes");

            migrationBuilder.DropTable(
                name: "Clients");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
