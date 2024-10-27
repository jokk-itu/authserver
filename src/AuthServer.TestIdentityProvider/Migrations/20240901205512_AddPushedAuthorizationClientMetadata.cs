using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuthServer.TestIdentityProvider.Migrations
{
    /// <inheritdoc />
    public partial class AddPushedAuthorizationClientMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RequestUriExpiration",
                table: "Client",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "RequirePushedAuthorizationRequests",
                table: "Client",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RequestUriExpiration",
                table: "Client");

            migrationBuilder.DropColumn(
                name: "RequirePushedAuthorizationRequests",
                table: "Client");
        }
    }
}
