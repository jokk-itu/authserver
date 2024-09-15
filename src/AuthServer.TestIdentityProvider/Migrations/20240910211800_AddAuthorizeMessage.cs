using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuthServer.TestIdentityProvider.Migrations
{
    /// <inheritdoc />
    public partial class AddAuthorizeMessage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "HashedValue",
                table: "Nonce",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "AuthorizeMessage",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Reference = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", maxLength: 4096, nullable: false),
                    RedeemedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ClientId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthorizeMessage", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuthorizeMessage_Client_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Client",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuthorizeMessage_ClientId",
                table: "AuthorizeMessage",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_AuthorizeMessage_Reference",
                table: "AuthorizeMessage",
                column: "Reference",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuthorizeMessage");

            migrationBuilder.DropColumn(
                name: "HashedValue",
                table: "Nonce");
        }
    }
}
