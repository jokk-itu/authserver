using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuthServer.TestIdentityProvider.Migrations
{
    /// <inheritdoc />
    public partial class AddAuthenticationContextReference : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DefaultAcrValues",
                table: "Client");

            migrationBuilder.AddColumn<int>(
                name: "AuthenticationContextReferenceId",
                table: "AuthenticationMethodReference",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AuthenticationContextReference",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthenticationContextReference", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ClientAuthenticationContextReference",
                columns: table => new
                {
                    ClientId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    AuthenticationContextReferenceId = table.Column<int>(type: "int", nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientAuthenticationContextReference", x => new { x.ClientId, x.AuthenticationContextReferenceId });
                    table.ForeignKey(
                        name: "FK_ClientAuthenticationContextReference_AuthenticationContextReference_AuthenticationContextReferenceId",
                        column: x => x.AuthenticationContextReferenceId,
                        principalTable: "AuthenticationContextReference",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClientAuthenticationContextReference_Client_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Client",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "AuthenticationMethodReference",
                keyColumn: "Id",
                keyValue: 1,
                column: "AuthenticationContextReferenceId",
                value: null);

            migrationBuilder.UpdateData(
                table: "AuthenticationMethodReference",
                keyColumn: "Id",
                keyValue: 2,
                column: "AuthenticationContextReferenceId",
                value: null);

            migrationBuilder.UpdateData(
                table: "AuthenticationMethodReference",
                keyColumn: "Id",
                keyValue: 3,
                column: "AuthenticationContextReferenceId",
                value: null);

            migrationBuilder.UpdateData(
                table: "AuthenticationMethodReference",
                keyColumn: "Id",
                keyValue: 4,
                column: "AuthenticationContextReferenceId",
                value: null);

            migrationBuilder.UpdateData(
                table: "AuthenticationMethodReference",
                keyColumn: "Id",
                keyValue: 5,
                column: "AuthenticationContextReferenceId",
                value: null);

            migrationBuilder.UpdateData(
                table: "AuthenticationMethodReference",
                keyColumn: "Id",
                keyValue: 6,
                column: "AuthenticationContextReferenceId",
                value: null);

            migrationBuilder.UpdateData(
                table: "AuthenticationMethodReference",
                keyColumn: "Id",
                keyValue: 7,
                column: "AuthenticationContextReferenceId",
                value: null);

            migrationBuilder.UpdateData(
                table: "AuthenticationMethodReference",
                keyColumn: "Id",
                keyValue: 8,
                column: "AuthenticationContextReferenceId",
                value: null);

            migrationBuilder.UpdateData(
                table: "AuthenticationMethodReference",
                keyColumn: "Id",
                keyValue: 9,
                column: "AuthenticationContextReferenceId",
                value: null);

            migrationBuilder.UpdateData(
                table: "AuthenticationMethodReference",
                keyColumn: "Id",
                keyValue: 10,
                column: "AuthenticationContextReferenceId",
                value: null);

            migrationBuilder.UpdateData(
                table: "AuthenticationMethodReference",
                keyColumn: "Id",
                keyValue: 11,
                column: "AuthenticationContextReferenceId",
                value: null);

            migrationBuilder.UpdateData(
                table: "AuthenticationMethodReference",
                keyColumn: "Id",
                keyValue: 12,
                column: "AuthenticationContextReferenceId",
                value: null);

            migrationBuilder.UpdateData(
                table: "AuthenticationMethodReference",
                keyColumn: "Id",
                keyValue: 13,
                column: "AuthenticationContextReferenceId",
                value: null);

            migrationBuilder.UpdateData(
                table: "AuthenticationMethodReference",
                keyColumn: "Id",
                keyValue: 14,
                column: "AuthenticationContextReferenceId",
                value: null);

            migrationBuilder.UpdateData(
                table: "AuthenticationMethodReference",
                keyColumn: "Id",
                keyValue: 15,
                column: "AuthenticationContextReferenceId",
                value: null);

            migrationBuilder.UpdateData(
                table: "AuthenticationMethodReference",
                keyColumn: "Id",
                keyValue: 16,
                column: "AuthenticationContextReferenceId",
                value: null);

            migrationBuilder.UpdateData(
                table: "AuthenticationMethodReference",
                keyColumn: "Id",
                keyValue: 17,
                column: "AuthenticationContextReferenceId",
                value: null);

            migrationBuilder.UpdateData(
                table: "AuthenticationMethodReference",
                keyColumn: "Id",
                keyValue: 18,
                column: "AuthenticationContextReferenceId",
                value: null);

            migrationBuilder.UpdateData(
                table: "AuthenticationMethodReference",
                keyColumn: "Id",
                keyValue: 19,
                column: "AuthenticationContextReferenceId",
                value: null);

            migrationBuilder.UpdateData(
                table: "AuthenticationMethodReference",
                keyColumn: "Id",
                keyValue: 20,
                column: "AuthenticationContextReferenceId",
                value: null);

            migrationBuilder.UpdateData(
                table: "AuthenticationMethodReference",
                keyColumn: "Id",
                keyValue: 21,
                column: "AuthenticationContextReferenceId",
                value: null);

            migrationBuilder.CreateIndex(
                name: "IX_AuthenticationMethodReference_AuthenticationContextReferenceId",
                table: "AuthenticationMethodReference",
                column: "AuthenticationContextReferenceId");

            migrationBuilder.CreateIndex(
                name: "IX_AuthenticationContextReference_Name",
                table: "AuthenticationContextReference",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClientAuthenticationContextReference_AuthenticationContextReferenceId",
                table: "ClientAuthenticationContextReference",
                column: "AuthenticationContextReferenceId");

            migrationBuilder.AddForeignKey(
                name: "FK_AuthenticationMethodReference_AuthenticationContextReference_AuthenticationContextReferenceId",
                table: "AuthenticationMethodReference",
                column: "AuthenticationContextReferenceId",
                principalTable: "AuthenticationContextReference",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AuthenticationMethodReference_AuthenticationContextReference_AuthenticationContextReferenceId",
                table: "AuthenticationMethodReference");

            migrationBuilder.DropTable(
                name: "ClientAuthenticationContextReference");

            migrationBuilder.DropTable(
                name: "AuthenticationContextReference");

            migrationBuilder.DropIndex(
                name: "IX_AuthenticationMethodReference_AuthenticationContextReferenceId",
                table: "AuthenticationMethodReference");

            migrationBuilder.DropColumn(
                name: "AuthenticationContextReferenceId",
                table: "AuthenticationMethodReference");

            migrationBuilder.AddColumn<string>(
                name: "DefaultAcrValues",
                table: "Client",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);
        }
    }
}
