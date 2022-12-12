using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    public partial class Initial : Migration
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
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Secret = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TosUri = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PolicyUri = table.Column<string>(type: "nvarchar(max)", nullable: true),
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
                name: "Sessions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaxAge = table.Column<long>(type: "bigint", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Updated = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sessions", x => x.Id);
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
                name: "AuthorizationCodeGrants",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Nonce = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsRedeemed = table.Column<bool>(type: "bit", nullable: false),
                    AuthTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SessionId = table.Column<long>(type: "bigint", nullable: true),
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

            migrationBuilder.CreateTable(
                name: "SessionClients",
                columns: table => new
                {
                    ClientsId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    SessionsId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SessionClients", x => new { x.ClientsId, x.SessionsId });
                    table.ForeignKey(
                        name: "FK_SessionClients_Clients_ClientsId",
                        column: x => x.ClientsId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SessionClients_Sessions_SessionsId",
                        column: x => x.SessionsId,
                        principalTable: "Sessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tokens",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsRevoked = table.Column<bool>(type: "bit", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TokenType = table.Column<int>(type: "int", nullable: false),
                    AccessToken_SessionId = table.Column<long>(type: "bigint", nullable: true),
                    AccessToken_ClientId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    ClientRegistrationToken_ClientId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    IdToken_SessionId = table.Column<long>(type: "bigint", nullable: true),
                    IdToken_ClientId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    SessionId = table.Column<long>(type: "bigint", nullable: true),
                    ClientId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    ResourceId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    ScopeId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tokens_Clients_AccessToken_ClientId",
                        column: x => x.AccessToken_ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Tokens_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Tokens_Clients_ClientRegistrationToken_ClientId",
                        column: x => x.ClientRegistrationToken_ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Tokens_Clients_IdToken_ClientId",
                        column: x => x.IdToken_ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Tokens_Resources_ResourceId",
                        column: x => x.ResourceId,
                        principalTable: "Resources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Tokens_Scopes_ScopeId",
                        column: x => x.ScopeId,
                        principalTable: "Scopes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Tokens_Sessions_AccessToken_SessionId",
                        column: x => x.AccessToken_SessionId,
                        principalTable: "Sessions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Tokens_Sessions_IdToken_SessionId",
                        column: x => x.IdToken_SessionId,
                        principalTable: "Sessions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Tokens_Sessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "Sessions",
                        principalColumn: "Id");
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
                    Locale = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SessionId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_Sessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "Sessions",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ConsentGrants",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IssuedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
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
                name: "UserTokens",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsRedeemed = table.Column<bool>(type: "bit", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserTokens_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ConsentedGrantClaims",
                columns: table => new
                {
                    ConsentGrantsId = table.Column<long>(type: "bigint", nullable: false),
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
                    ConsentGrantsId = table.Column<long>(type: "bigint", nullable: false),
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
                name: "IX_SessionClients_SessionsId",
                table: "SessionClients",
                column: "SessionsId");

            migrationBuilder.CreateIndex(
                name: "IX_Tokens_AccessToken_ClientId",
                table: "Tokens",
                column: "AccessToken_ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_Tokens_AccessToken_SessionId",
                table: "Tokens",
                column: "AccessToken_SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_Tokens_ClientId",
                table: "Tokens",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_Tokens_ClientRegistrationToken_ClientId",
                table: "Tokens",
                column: "ClientRegistrationToken_ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_Tokens_IdToken_ClientId",
                table: "Tokens",
                column: "IdToken_ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_Tokens_IdToken_SessionId",
                table: "Tokens",
                column: "IdToken_SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_Tokens_ResourceId",
                table: "Tokens",
                column: "ResourceId");

            migrationBuilder.CreateIndex(
                name: "IX_Tokens_ScopeId",
                table: "Tokens",
                column: "ScopeId");

            migrationBuilder.CreateIndex(
                name: "IX_Tokens_SessionId",
                table: "Tokens",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_SessionId",
                table: "Users",
                column: "SessionId",
                unique: true,
                filter: "[SessionId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Users_UserName",
                table: "Users",
                column: "UserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserTokens_UserId",
                table: "UserTokens",
                column: "UserId");
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
                name: "SessionClients");

            migrationBuilder.DropTable(
                name: "Tokens");

            migrationBuilder.DropTable(
                name: "UserTokens");

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

            migrationBuilder.DropTable(
                name: "Sessions");
        }
    }
}
