using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AuthServer.TestIdentityProvider.Migrations
{
    /// <inheritdoc />
    public partial class AddAuthenticationReferences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DefaultAcrValues",
                table: "Client");

            migrationBuilder.AddColumn<int>(
                name: "AuthenticationContextReferenceId",
                table: "AuthorizationGrant",
                type: "int",
                nullable: false,
                defaultValue: 0);

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
                name: "AuthenticationMethodReference",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthenticationMethodReference", x => x.Id);
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
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ClientAuthenticationContextReference_Client_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Client",
                        principalColumn: "Id");
                });

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

            migrationBuilder.InsertData(
                table: "AuthenticationMethodReference",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "pwd" },
                    { 2, "mfa" },
                    { 3, "sms" },
                    { 4, "face" },
                    { 5, "fpt" },
                    { 6, "geo" },
                    { 7, "iris" },
                    { 8, "kba" },
                    { 9, "mca" },
                    { 10, "otp" },
                    { 11, "pin" },
                    { 12, "hwk" },
                    { 13, "pop" },
                    { 14, "swk" },
                    { 15, "retina" },
                    { 16, "rba" },
                    { 17, "sc" },
                    { 18, "tel" },
                    { 19, "user" },
                    { 20, "vbm" },
                    { 21, "wia" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuthorizationGrant_AuthenticationContextReferenceId",
                table: "AuthorizationGrant",
                column: "AuthenticationContextReferenceId");

            migrationBuilder.CreateIndex(
                name: "IX_AuthenticationContextReference_Name",
                table: "AuthenticationContextReference",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AuthenticationMethodReference_Name",
                table: "AuthenticationMethodReference",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AuthenticationMethodReferenceAuthorizationGrant_AuthorizationGrantsId",
                table: "AuthenticationMethodReferenceAuthorizationGrant",
                column: "AuthorizationGrantsId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientAuthenticationContextReference_AuthenticationContextReferenceId",
                table: "ClientAuthenticationContextReference",
                column: "AuthenticationContextReferenceId");

            migrationBuilder.AddForeignKey(
                name: "FK_AuthorizationGrant_AuthenticationContextReference_AuthenticationContextReferenceId",
                table: "AuthorizationGrant",
                column: "AuthenticationContextReferenceId",
                principalTable: "AuthenticationContextReference",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AuthorizationGrant_AuthenticationContextReference_AuthenticationContextReferenceId",
                table: "AuthorizationGrant");

            migrationBuilder.DropTable(
                name: "AuthenticationMethodReferenceAuthorizationGrant");

            migrationBuilder.DropTable(
                name: "ClientAuthenticationContextReference");

            migrationBuilder.DropTable(
                name: "AuthenticationMethodReference");

            migrationBuilder.DropTable(
                name: "AuthenticationContextReference");

            migrationBuilder.DropIndex(
                name: "IX_AuthorizationGrant_AuthenticationContextReferenceId",
                table: "AuthorizationGrant");

            migrationBuilder.DropColumn(
                name: "AuthenticationContextReferenceId",
                table: "AuthorizationGrant");

            migrationBuilder.AddColumn<string>(
                name: "DefaultAcrValues",
                table: "Client",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);
        }
    }
}
