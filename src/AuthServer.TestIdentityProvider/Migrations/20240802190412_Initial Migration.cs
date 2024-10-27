using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AuthServer.TestIdentityProvider.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Claim",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
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
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SecretHash = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    SecretExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SecretExpiration = table.Column<int>(type: "int", nullable: true),
                    AccessTokenExpiration = table.Column<int>(type: "int", nullable: false),
                    RefreshTokenExpiration = table.Column<int>(type: "int", nullable: true),
                    AuthorizationCodeExpiration = table.Column<int>(type: "int", nullable: true),
                    JwksExpiration = table.Column<int>(type: "int", nullable: true),
                    DefaultAcrValues = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    TosUri = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    PolicyUri = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ClientUri = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    LogoUri = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    InitiateLoginUri = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    BackchannelLogoutUri = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    JwksUri = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Jwks = table.Column<string>(type: "nvarchar(max)", maxLength: 2147483647, nullable: true),
                    JwksExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RequireReferenceToken = table.Column<bool>(type: "bit", nullable: false),
                    RequireConsent = table.Column<bool>(type: "bit", nullable: false),
                    RequireSignedRequestObject = table.Column<bool>(type: "bit", nullable: false),
                    DefaultMaxAge = table.Column<int>(type: "int", nullable: true),
                    ApplicationType = table.Column<int>(type: "int", nullable: false),
                    TokenEndpointAuthMethod = table.Column<int>(type: "int", nullable: false),
                    SubjectType = table.Column<int>(type: "int", nullable: true),
                    TokenEndpointAuthSigningAlg = table.Column<int>(type: "int", nullable: false),
                    UserinfoEncryptedResponseEnc = table.Column<int>(type: "int", nullable: true),
                    UserinfoEncryptedResponseAlg = table.Column<int>(type: "int", nullable: true),
                    UserinfoSignedResponseAlg = table.Column<int>(type: "int", nullable: true),
                    RequestObjectEncryptionEnc = table.Column<int>(type: "int", nullable: true),
                    RequestObjectEncryptionAlg = table.Column<int>(type: "int", nullable: true),
                    RequestObjectSigningAlg = table.Column<int>(type: "int", nullable: true),
                    IdTokenEncryptedResponseEnc = table.Column<int>(type: "int", nullable: true),
                    IdTokenEncryptedResponseAlg = table.Column<int>(type: "int", nullable: true),
                    IdTokenSignedResponseAlg = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Client", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GrantType",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GrantType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ResponseType",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
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
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Scope", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Contact",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ClientId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contact", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Contact_Client_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Client",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PostLogoutRedirectUri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Uri = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ClientId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PostLogoutRedirectUri", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PostLogoutRedirectUri_Client_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Client",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "RedirectUri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Uri = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ClientId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RedirectUri", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RedirectUri_Client_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Client",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "RequestUri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Uri = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ClientId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RequestUri", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RequestUri_Client_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Client",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SubjectIdentifier",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    ClientId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    PublicSubjectIdentifierId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubjectIdentifier", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubjectIdentifier_Client_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Client",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SubjectIdentifier_SubjectIdentifier_PublicSubjectIdentifierId",
                        column: x => x.PublicSubjectIdentifierId,
                        principalTable: "SubjectIdentifier",
                        principalColumn: "Id");
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
                name: "ConsentGrant",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClientId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    PublicSubjectIdentifierId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsentGrant", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConsentGrant_Client_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Client",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ConsentGrant_SubjectIdentifier_PublicSubjectIdentifierId",
                        column: x => x.PublicSubjectIdentifierId,
                        principalTable: "SubjectIdentifier",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Session",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RevokedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PublicSubjectIdentifierId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Session", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Session_SubjectIdentifier_PublicSubjectIdentifierId",
                        column: x => x.PublicSubjectIdentifierId,
                        principalTable: "SubjectIdentifier",
                        principalColumn: "Id");
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
                name: "AuthorizationGrant",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    AuthTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MaxAge = table.Column<long>(type: "bigint", nullable: true),
                    RevokedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SessionId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClientId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    SubjectIdentifierId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthorizationGrant", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuthorizationGrant_Client_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Client",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AuthorizationGrant_Session_SessionId",
                        column: x => x.SessionId,
                        principalTable: "Session",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AuthorizationGrant_SubjectIdentifier_SubjectIdentifierId",
                        column: x => x.SubjectIdentifierId,
                        principalTable: "SubjectIdentifier",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AuthorizationCode",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    IssuedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RedeemedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AuthorizationGrantId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthorizationCode", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuthorizationCode_AuthorizationGrant_AuthorizationGrantId",
                        column: x => x.AuthorizationGrantId,
                        principalTable: "AuthorizationGrant",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Nonce",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", maxLength: 2147483647, nullable: false),
                    IssuedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AuthorizationGrantId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Nonce", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Nonce_AuthorizationGrant_AuthorizationGrantId",
                        column: x => x.AuthorizationGrantId,
                        principalTable: "AuthorizationGrant",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Token",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Reference = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Scope = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    TokenType = table.Column<int>(type: "int", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IssuedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NotBefore = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Audience = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Issuer = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    RevokedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ClientId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    AuthorizationGrantId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Token", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Token_AuthorizationGrant_AuthorizationGrantId",
                        column: x => x.AuthorizationGrantId,
                        principalTable: "AuthorizationGrant",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Token_Client_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Client",
                        principalColumn: "Id");
                });

            migrationBuilder.InsertData(
                table: "Claim",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "name" },
                    { 2, "given_name" },
                    { 3, "family_name" },
                    { 4, "middle_name" },
                    { 5, "nickname" },
                    { 6, "preferred_username" },
                    { 7, "profile" },
                    { 8, "picture" },
                    { 9, "website" },
                    { 10, "email" },
                    { 11, "email_verified" },
                    { 12, "gender" },
                    { 13, "birthdate" },
                    { 14, "zoneinfo" },
                    { 15, "locale" },
                    { 16, "phone_number" },
                    { 17, "phone_number_verified" },
                    { 18, "address" },
                    { 19, "updated_at" },
                    { 20, "roles" }
                });

            migrationBuilder.InsertData(
                table: "GrantType",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "authorization_code" },
                    { 2, "client_credentials" },
                    { 3, "refresh_token" }
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
                    { 2, "offline_access" },
                    { 3, "profile" },
                    { 4, "address" },
                    { 5, "email" },
                    { 6, "phone" },
                    { 7, "authserver:userinfo" },
                    { 8, "authserver:register" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuthorizationCode_AuthorizationGrantId",
                table: "AuthorizationCode",
                column: "AuthorizationGrantId");

            migrationBuilder.CreateIndex(
                name: "IX_AuthorizationGrant_ClientId",
                table: "AuthorizationGrant",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_AuthorizationGrant_SessionId",
                table: "AuthorizationGrant",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_AuthorizationGrant_SubjectIdentifierId",
                table: "AuthorizationGrant",
                column: "SubjectIdentifierId");

            migrationBuilder.CreateIndex(
                name: "IX_Claim_Name",
                table: "Claim",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClaimConsentGrant_ConsentedClaimsId",
                table: "ClaimConsentGrant",
                column: "ConsentedClaimsId");

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
                name: "IX_ConsentGrant_PublicSubjectIdentifierId",
                table: "ConsentGrant",
                column: "PublicSubjectIdentifierId");

            migrationBuilder.CreateIndex(
                name: "IX_ConsentGrantScope_ConsentedScopesId",
                table: "ConsentGrantScope",
                column: "ConsentedScopesId");

            migrationBuilder.CreateIndex(
                name: "IX_Contact_ClientId",
                table: "Contact",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_GrantType_Name",
                table: "GrantType",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Nonce_AuthorizationGrantId",
                table: "Nonce",
                column: "AuthorizationGrantId");

            migrationBuilder.CreateIndex(
                name: "IX_PostLogoutRedirectUri_ClientId",
                table: "PostLogoutRedirectUri",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_RedirectUri_ClientId",
                table: "RedirectUri",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_RequestUri_ClientId",
                table: "RequestUri",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_ResponseType_Name",
                table: "ResponseType",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Scope_Name",
                table: "Scope",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Session_PublicSubjectIdentifierId",
                table: "Session",
                column: "PublicSubjectIdentifierId");

            migrationBuilder.CreateIndex(
                name: "IX_SubjectIdentifier_ClientId",
                table: "SubjectIdentifier",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_SubjectIdentifier_PublicSubjectIdentifierId",
                table: "SubjectIdentifier",
                column: "PublicSubjectIdentifierId");

            migrationBuilder.CreateIndex(
                name: "IX_Token_AuthorizationGrantId",
                table: "Token",
                column: "AuthorizationGrantId");

            migrationBuilder.CreateIndex(
                name: "IX_Token_ClientId",
                table: "Token",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_Token_Reference",
                table: "Token",
                column: "Reference",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuthorizationCode");

            migrationBuilder.DropTable(
                name: "ClaimConsentGrant");

            migrationBuilder.DropTable(
                name: "ClientGrantType");

            migrationBuilder.DropTable(
                name: "ClientResponseType");

            migrationBuilder.DropTable(
                name: "ClientScope");

            migrationBuilder.DropTable(
                name: "ConsentGrantScope");

            migrationBuilder.DropTable(
                name: "Contact");

            migrationBuilder.DropTable(
                name: "Nonce");

            migrationBuilder.DropTable(
                name: "PostLogoutRedirectUri");

            migrationBuilder.DropTable(
                name: "RedirectUri");

            migrationBuilder.DropTable(
                name: "RequestUri");

            migrationBuilder.DropTable(
                name: "Token");

            migrationBuilder.DropTable(
                name: "Claim");

            migrationBuilder.DropTable(
                name: "GrantType");

            migrationBuilder.DropTable(
                name: "ResponseType");

            migrationBuilder.DropTable(
                name: "ConsentGrant");

            migrationBuilder.DropTable(
                name: "Scope");

            migrationBuilder.DropTable(
                name: "AuthorizationGrant");

            migrationBuilder.DropTable(
                name: "Session");

            migrationBuilder.DropTable(
                name: "SubjectIdentifier");

            migrationBuilder.DropTable(
                name: "Client");
        }
    }
}
