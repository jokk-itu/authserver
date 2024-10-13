using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuthServer.TestIdentityProvider.Migrations
{
    /// <inheritdoc />
    public partial class RenameManyToManyRelations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ClientGrantType_Client_ClientsId",
                table: "ClientGrantType");

            migrationBuilder.DropForeignKey(
                name: "FK_ClientGrantType_GrantType_GrantTypesId",
                table: "ClientGrantType");

            migrationBuilder.DropForeignKey(
                name: "FK_ClientResponseType_Client_ClientsId",
                table: "ClientResponseType");

            migrationBuilder.DropForeignKey(
                name: "FK_ClientResponseType_ResponseType_ResponseTypesId",
                table: "ClientResponseType");

            migrationBuilder.DropForeignKey(
                name: "FK_ClientScope_Client_ClientsId",
                table: "ClientScope");

            migrationBuilder.DropForeignKey(
                name: "FK_ClientScope_Scope_ScopesId",
                table: "ClientScope");

            migrationBuilder.DropForeignKey(
                name: "FK_ConsentGrantScope_ConsentGrant_ConsentGrantsId",
                table: "ConsentGrantScope");

            migrationBuilder.DropForeignKey(
                name: "FK_ConsentGrantScope_Scope_ConsentedScopesId",
                table: "ConsentGrantScope");

            migrationBuilder.DropTable(
                name: "AuthenticationMethodReferenceAuthorizationGrant");

            migrationBuilder.DropTable(
                name: "ClaimConsentGrant");

            migrationBuilder.RenameColumn(
                name: "ConsentedScopesId",
                table: "ConsentGrantScope",
                newName: "ScopeId");

            migrationBuilder.RenameColumn(
                name: "ConsentGrantsId",
                table: "ConsentGrantScope",
                newName: "ConsentGrantId");

            migrationBuilder.RenameIndex(
                name: "IX_ConsentGrantScope_ConsentedScopesId",
                table: "ConsentGrantScope",
                newName: "IX_ConsentGrantScope_ScopeId");

            migrationBuilder.RenameColumn(
                name: "ScopesId",
                table: "ClientScope",
                newName: "ScopeId");

            migrationBuilder.RenameColumn(
                name: "ClientsId",
                table: "ClientScope",
                newName: "ClientId");

            migrationBuilder.RenameIndex(
                name: "IX_ClientScope_ScopesId",
                table: "ClientScope",
                newName: "IX_ClientScope_ScopeId");

            migrationBuilder.RenameColumn(
                name: "ResponseTypesId",
                table: "ClientResponseType",
                newName: "ResponseTypeId");

            migrationBuilder.RenameColumn(
                name: "ClientsId",
                table: "ClientResponseType",
                newName: "ClientId");

            migrationBuilder.RenameIndex(
                name: "IX_ClientResponseType_ResponseTypesId",
                table: "ClientResponseType",
                newName: "IX_ClientResponseType_ResponseTypeId");

            migrationBuilder.RenameColumn(
                name: "GrantTypesId",
                table: "ClientGrantType",
                newName: "GrantTypeId");

            migrationBuilder.RenameColumn(
                name: "ClientsId",
                table: "ClientGrantType",
                newName: "ClientId");

            migrationBuilder.RenameIndex(
                name: "IX_ClientGrantType_GrantTypesId",
                table: "ClientGrantType",
                newName: "IX_ClientGrantType_GrantTypeId");

            migrationBuilder.CreateTable(
                name: "AuthorizationGrantAuthenticationMethodReference",
                columns: table => new
                {
                    AuthenticationMethodReferenceId = table.Column<int>(type: "int", nullable: false),
                    AuthorizationGrantId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthorizationGrantAuthenticationMethodReference", x => new { x.AuthenticationMethodReferenceId, x.AuthorizationGrantId });
                    table.ForeignKey(
                        name: "FK_AuthorizationGrantAuthenticationMethodReference_AuthenticationMethodReference_AuthenticationMethodReferenceId",
                        column: x => x.AuthenticationMethodReferenceId,
                        principalTable: "AuthenticationMethodReference",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AuthorizationGrantAuthenticationMethodReference_AuthorizationGrant_AuthorizationGrantId",
                        column: x => x.AuthorizationGrantId,
                        principalTable: "AuthorizationGrant",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ConsentGrantClaim",
                columns: table => new
                {
                    ClaimId = table.Column<int>(type: "int", nullable: false),
                    ConsentGrantId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsentGrantClaim", x => new { x.ClaimId, x.ConsentGrantId });
                    table.ForeignKey(
                        name: "FK_ConsentGrantClaim_Claim_ClaimId",
                        column: x => x.ClaimId,
                        principalTable: "Claim",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ConsentGrantClaim_ConsentGrant_ConsentGrantId",
                        column: x => x.ConsentGrantId,
                        principalTable: "ConsentGrant",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuthorizationGrantAuthenticationMethodReference_AuthorizationGrantId",
                table: "AuthorizationGrantAuthenticationMethodReference",
                column: "AuthorizationGrantId");

            migrationBuilder.CreateIndex(
                name: "IX_ConsentGrantClaim_ConsentGrantId",
                table: "ConsentGrantClaim",
                column: "ConsentGrantId");

            migrationBuilder.AddForeignKey(
                name: "FK_ClientGrantType_Client_ClientId",
                table: "ClientGrantType",
                column: "ClientId",
                principalTable: "Client",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ClientGrantType_GrantType_GrantTypeId",
                table: "ClientGrantType",
                column: "GrantTypeId",
                principalTable: "GrantType",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ClientResponseType_Client_ClientId",
                table: "ClientResponseType",
                column: "ClientId",
                principalTable: "Client",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ClientResponseType_ResponseType_ResponseTypeId",
                table: "ClientResponseType",
                column: "ResponseTypeId",
                principalTable: "ResponseType",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ClientScope_Client_ClientId",
                table: "ClientScope",
                column: "ClientId",
                principalTable: "Client",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ClientScope_Scope_ScopeId",
                table: "ClientScope",
                column: "ScopeId",
                principalTable: "Scope",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ConsentGrantScope_ConsentGrant_ConsentGrantId",
                table: "ConsentGrantScope",
                column: "ConsentGrantId",
                principalTable: "ConsentGrant",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ConsentGrantScope_Scope_ScopeId",
                table: "ConsentGrantScope",
                column: "ScopeId",
                principalTable: "Scope",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ClientGrantType_Client_ClientId",
                table: "ClientGrantType");

            migrationBuilder.DropForeignKey(
                name: "FK_ClientGrantType_GrantType_GrantTypeId",
                table: "ClientGrantType");

            migrationBuilder.DropForeignKey(
                name: "FK_ClientResponseType_Client_ClientId",
                table: "ClientResponseType");

            migrationBuilder.DropForeignKey(
                name: "FK_ClientResponseType_ResponseType_ResponseTypeId",
                table: "ClientResponseType");

            migrationBuilder.DropForeignKey(
                name: "FK_ClientScope_Client_ClientId",
                table: "ClientScope");

            migrationBuilder.DropForeignKey(
                name: "FK_ClientScope_Scope_ScopeId",
                table: "ClientScope");

            migrationBuilder.DropForeignKey(
                name: "FK_ConsentGrantScope_ConsentGrant_ConsentGrantId",
                table: "ConsentGrantScope");

            migrationBuilder.DropForeignKey(
                name: "FK_ConsentGrantScope_Scope_ScopeId",
                table: "ConsentGrantScope");

            migrationBuilder.DropTable(
                name: "AuthorizationGrantAuthenticationMethodReference");

            migrationBuilder.DropTable(
                name: "ConsentGrantClaim");

            migrationBuilder.RenameColumn(
                name: "ScopeId",
                table: "ConsentGrantScope",
                newName: "ConsentedScopesId");

            migrationBuilder.RenameColumn(
                name: "ConsentGrantId",
                table: "ConsentGrantScope",
                newName: "ConsentGrantsId");

            migrationBuilder.RenameIndex(
                name: "IX_ConsentGrantScope_ScopeId",
                table: "ConsentGrantScope",
                newName: "IX_ConsentGrantScope_ConsentedScopesId");

            migrationBuilder.RenameColumn(
                name: "ScopeId",
                table: "ClientScope",
                newName: "ScopesId");

            migrationBuilder.RenameColumn(
                name: "ClientId",
                table: "ClientScope",
                newName: "ClientsId");

            migrationBuilder.RenameIndex(
                name: "IX_ClientScope_ScopeId",
                table: "ClientScope",
                newName: "IX_ClientScope_ScopesId");

            migrationBuilder.RenameColumn(
                name: "ResponseTypeId",
                table: "ClientResponseType",
                newName: "ResponseTypesId");

            migrationBuilder.RenameColumn(
                name: "ClientId",
                table: "ClientResponseType",
                newName: "ClientsId");

            migrationBuilder.RenameIndex(
                name: "IX_ClientResponseType_ResponseTypeId",
                table: "ClientResponseType",
                newName: "IX_ClientResponseType_ResponseTypesId");

            migrationBuilder.RenameColumn(
                name: "GrantTypeId",
                table: "ClientGrantType",
                newName: "GrantTypesId");

            migrationBuilder.RenameColumn(
                name: "ClientId",
                table: "ClientGrantType",
                newName: "ClientsId");

            migrationBuilder.RenameIndex(
                name: "IX_ClientGrantType_GrantTypeId",
                table: "ClientGrantType",
                newName: "IX_ClientGrantType_GrantTypesId");

            migrationBuilder.CreateTable(
                name: "AuthenticationMethodReferenceAuthorizationGrant",
                columns: table => new
                {
                    AuthenticationMethodReferencesId = table.Column<int>(type: "int", nullable: false),
                    AuthorizationGrantsId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthenticationMethodReferenceAuthorizationGrant", x => new { x.AuthenticationMethodReferencesId, x.AuthorizationGrantsId });
                    table.ForeignKey(
                        name: "FK_AuthenticationMethodReferenceAuthorizationGrant_AuthenticationMethodReference_AuthenticationMethodReferencesId",
                        column: x => x.AuthenticationMethodReferencesId,
                        principalTable: "AuthenticationMethodReference",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AuthenticationMethodReferenceAuthorizationGrant_AuthorizationGrant_AuthorizationGrantsId",
                        column: x => x.AuthorizationGrantsId,
                        principalTable: "AuthorizationGrant",
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

            migrationBuilder.CreateIndex(
                name: "IX_AuthenticationMethodReferenceAuthorizationGrant_AuthorizationGrantsId",
                table: "AuthenticationMethodReferenceAuthorizationGrant",
                column: "AuthorizationGrantsId");

            migrationBuilder.CreateIndex(
                name: "IX_ClaimConsentGrant_ConsentedClaimsId",
                table: "ClaimConsentGrant",
                column: "ConsentedClaimsId");

            migrationBuilder.AddForeignKey(
                name: "FK_ClientGrantType_Client_ClientsId",
                table: "ClientGrantType",
                column: "ClientsId",
                principalTable: "Client",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ClientGrantType_GrantType_GrantTypesId",
                table: "ClientGrantType",
                column: "GrantTypesId",
                principalTable: "GrantType",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ClientResponseType_Client_ClientsId",
                table: "ClientResponseType",
                column: "ClientsId",
                principalTable: "Client",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ClientResponseType_ResponseType_ResponseTypesId",
                table: "ClientResponseType",
                column: "ResponseTypesId",
                principalTable: "ResponseType",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ClientScope_Client_ClientsId",
                table: "ClientScope",
                column: "ClientsId",
                principalTable: "Client",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ClientScope_Scope_ScopesId",
                table: "ClientScope",
                column: "ScopesId",
                principalTable: "Scope",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ConsentGrantScope_ConsentGrant_ConsentGrantsId",
                table: "ConsentGrantScope",
                column: "ConsentGrantsId",
                principalTable: "ConsentGrant",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ConsentGrantScope_Scope_ConsentedScopesId",
                table: "ConsentGrantScope",
                column: "ConsentedScopesId",
                principalTable: "Scope",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
