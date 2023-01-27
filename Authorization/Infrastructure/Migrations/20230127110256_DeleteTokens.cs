using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    public partial class DeleteTokens : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Tokens");

            migrationBuilder.DropTable(
                name: "UserTokens");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Tokens",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsRevoked = table.Column<bool>(type: "bit", nullable: false),
                    TokenType = table.Column<int>(type: "int", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AccessToken_ClientId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    AccessToken_SessionId = table.Column<long>(type: "bigint", nullable: true),
                    ClientRegistrationToken_ClientId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    IdToken_ClientId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    IdToken_SessionId = table.Column<long>(type: "bigint", nullable: true),
                    ClientId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    SessionId = table.Column<long>(type: "bigint", nullable: true),
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
                name: "UserTokens",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsRedeemed = table.Column<bool>(type: "bit", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
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
                name: "IX_UserTokens_UserId",
                table: "UserTokens",
                column: "UserId");
        }
    }
}
