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
                name: "Claim",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Claim", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Client",
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
                    BackChannelLogoutUri = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DefaultMaxAge = table.Column<long>(type: "bigint", nullable: true),
                    ApplicationType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TokenEndpointAuthMethod = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SubjectType = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Client", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Contact",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contact", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GrantType",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GrantType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Jwk",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedTimestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PrivateKey = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    Modulus = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    Exponent = table.Column<byte[]>(type: "varbinary(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Jwk", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Resource",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Secret = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Resource", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ResponseType",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResponseType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Scope",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Scope", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Birthdate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Locale = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RedirectUri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Uri = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ClientId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RedirectUri", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RedirectUri_Client_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Client",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ClientContact",
                columns: table => new
                {
                    ClientsId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ContactsId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientContact", x => new { x.ClientsId, x.ContactsId });
                    table.ForeignKey(
                        name: "FK_ClientContact_Client_ClientsId",
                        column: x => x.ClientsId,
                        principalTable: "Client",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClientContact_Contact_ContactsId",
                        column: x => x.ContactsId,
                        principalTable: "Contact",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ClientGrantType",
                columns: table => new
                {
                    ClientsId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    GrantTypesId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientGrantType", x => new { x.ClientsId, x.GrantTypesId });
                    table.ForeignKey(
                        name: "FK_ClientGrantType_Client_ClientsId",
                        column: x => x.ClientsId,
                        principalTable: "Client",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClientGrantType_GrantType_GrantTypesId",
                        column: x => x.GrantTypesId,
                        principalTable: "GrantType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ClientResponseType",
                columns: table => new
                {
                    ClientsId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ResponseTypesId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientResponseType", x => new { x.ClientsId, x.ResponseTypesId });
                    table.ForeignKey(
                        name: "FK_ClientResponseType_Client_ClientsId",
                        column: x => x.ClientsId,
                        principalTable: "Client",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClientResponseType_ResponseType_ResponseTypesId",
                        column: x => x.ResponseTypesId,
                        principalTable: "ResponseType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ClientScope",
                columns: table => new
                {
                    ClientsId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ScopesId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientScope", x => new { x.ClientsId, x.ScopesId });
                    table.ForeignKey(
                        name: "FK_ClientScope_Client_ClientsId",
                        column: x => x.ClientsId,
                        principalTable: "Client",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClientScope_Scope_ScopesId",
                        column: x => x.ScopesId,
                        principalTable: "Scope",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ResourceScope",
                columns: table => new
                {
                    ResourcesId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ScopesId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResourceScope", x => new { x.ResourcesId, x.ScopesId });
                    table.ForeignKey(
                        name: "FK_ResourceScope_Resource_ResourcesId",
                        column: x => x.ResourcesId,
                        principalTable: "Resource",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ResourceScope_Scope_ScopesId",
                        column: x => x.ScopesId,
                        principalTable: "Scope",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ConsentGrant",
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
                    table.PrimaryKey("PK_ConsentGrant", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConsentGrant_Client_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Client",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ConsentGrant_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Session",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    IsRevoked = table.Column<bool>(type: "bit", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Session", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Session_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ClaimConsentGrant",
                columns: table => new
                {
                    ConsentGrantsId = table.Column<int>(type: "int", nullable: false),
                    ConsentedClaimsId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClaimConsentGrant", x => new { x.ConsentGrantsId, x.ConsentedClaimsId });
                    table.ForeignKey(
                        name: "FK_ClaimConsentGrant_Claim_ConsentedClaimsId",
                        column: x => x.ConsentedClaimsId,
                        principalTable: "Claim",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClaimConsentGrant_ConsentGrant_ConsentGrantsId",
                        column: x => x.ConsentGrantsId,
                        principalTable: "ConsentGrant",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ConsentGrantScope",
                columns: table => new
                {
                    ConsentGrantsId = table.Column<int>(type: "int", nullable: false),
                    ConsentedScopesId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsentGrantScope", x => new { x.ConsentGrantsId, x.ConsentedScopesId });
                    table.ForeignKey(
                        name: "FK_ConsentGrantScope_ConsentGrant_ConsentGrantsId",
                        column: x => x.ConsentGrantsId,
                        principalTable: "ConsentGrant",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ConsentGrantScope_Scope_ConsentedScopesId",
                        column: x => x.ConsentedScopesId,
                        principalTable: "Scope",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AuthorizationCodeGrant",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    AuthTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MaxAge = table.Column<long>(type: "bigint", nullable: true),
                    IsRevoked = table.Column<bool>(type: "bit", nullable: false),
                    SessionId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    ClientId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthorizationCodeGrant", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuthorizationCodeGrant_Client_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Client",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AuthorizationCodeGrant_Session_SessionId",
                        column: x => x.SessionId,
                        principalTable: "Session",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AuthorizationCode",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsRedeemed = table.Column<bool>(type: "bit", nullable: false),
                    IssuedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RedeemedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AuthorizationCodeGrantId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthorizationCode", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuthorizationCode_AuthorizationCodeGrant_AuthorizationCodeGrantId",
                        column: x => x.AuthorizationCodeGrantId,
                        principalTable: "AuthorizationCodeGrant",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Nonce",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AuthorizationCodeGrantId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Nonce", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Nonce_AuthorizationCodeGrant_AuthorizationCodeGrantId",
                        column: x => x.AuthorizationCodeGrantId,
                        principalTable: "AuthorizationCodeGrant",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Token",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Reference = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Scope = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TokenType = table.Column<int>(type: "int", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IssuedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NotBefore = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Audience = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Issuer = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RevokedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AuthorizationGrantId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Token", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Token_AuthorizationCodeGrant_AuthorizationGrantId",
                        column: x => x.AuthorizationGrantId,
                        principalTable: "AuthorizationCodeGrant",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Claim",
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
                table: "GrantType",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "authorization_code" },
                    { 2, "refresh_token" },
                    { 3, "client_credentials" }
                });

            migrationBuilder.InsertData(
                table: "ResponseType",
                columns: new[] { "Id", "Name" },
                values: new object[] { 1, "code" });

            migrationBuilder.InsertData(
                table: "Scope",
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
                name: "IX_AuthorizationCode_AuthorizationCodeGrantId",
                table: "AuthorizationCode",
                column: "AuthorizationCodeGrantId");

            migrationBuilder.CreateIndex(
                name: "IX_AuthorizationCodeGrant_ClientId",
                table: "AuthorizationCodeGrant",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_AuthorizationCodeGrant_SessionId",
                table: "AuthorizationCodeGrant",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_ClaimConsentGrant_ConsentedClaimsId",
                table: "ClaimConsentGrant",
                column: "ConsentedClaimsId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientContact_ContactsId",
                table: "ClientContact",
                column: "ContactsId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientGrantType_GrantTypesId",
                table: "ClientGrantType",
                column: "GrantTypesId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientResponseType_ResponseTypesId",
                table: "ClientResponseType",
                column: "ResponseTypesId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientScope_ScopesId",
                table: "ClientScope",
                column: "ScopesId");

            migrationBuilder.CreateIndex(
                name: "IX_ConsentGrant_ClientId",
                table: "ConsentGrant",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_ConsentGrant_UserId",
                table: "ConsentGrant",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ConsentGrantScope_ConsentedScopesId",
                table: "ConsentGrantScope",
                column: "ConsentedScopesId");

            migrationBuilder.CreateIndex(
                name: "IX_Nonce_AuthorizationCodeGrantId",
                table: "Nonce",
                column: "AuthorizationCodeGrantId");

            migrationBuilder.CreateIndex(
                name: "IX_RedirectUri_ClientId",
                table: "RedirectUri",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_ResourceScope_ScopesId",
                table: "ResourceScope",
                column: "ScopesId");

            migrationBuilder.CreateIndex(
                name: "IX_Scope_Name",
                table: "Scope",
                column: "Name",
                unique: true,
                filter: "[Name] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Session_UserId",
                table: "Session",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Token_AuthorizationGrantId",
                table: "Token",
                column: "AuthorizationGrantId");

            migrationBuilder.CreateIndex(
                name: "IX_User_Email",
                table: "User",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_User_PhoneNumber",
                table: "User",
                column: "PhoneNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_User_UserName",
                table: "User",
                column: "UserName",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuthorizationCode");

            migrationBuilder.DropTable(
                name: "ClaimConsentGrant");

            migrationBuilder.DropTable(
                name: "ClientContact");

            migrationBuilder.DropTable(
                name: "ClientGrantType");

            migrationBuilder.DropTable(
                name: "ClientResponseType");

            migrationBuilder.DropTable(
                name: "ClientScope");

            migrationBuilder.DropTable(
                name: "ConsentGrantScope");

            migrationBuilder.DropTable(
                name: "Jwk");

            migrationBuilder.DropTable(
                name: "Nonce");

            migrationBuilder.DropTable(
                name: "RedirectUri");

            migrationBuilder.DropTable(
                name: "ResourceScope");

            migrationBuilder.DropTable(
                name: "Token");

            migrationBuilder.DropTable(
                name: "Claim");

            migrationBuilder.DropTable(
                name: "Contact");

            migrationBuilder.DropTable(
                name: "GrantType");

            migrationBuilder.DropTable(
                name: "ResponseType");

            migrationBuilder.DropTable(
                name: "ConsentGrant");

            migrationBuilder.DropTable(
                name: "Resource");

            migrationBuilder.DropTable(
                name: "Scope");

            migrationBuilder.DropTable(
                name: "AuthorizationCodeGrant");

            migrationBuilder.DropTable(
                name: "Client");

            migrationBuilder.DropTable(
                name: "Session");

            migrationBuilder.DropTable(
                name: "User");
        }
    }
}
