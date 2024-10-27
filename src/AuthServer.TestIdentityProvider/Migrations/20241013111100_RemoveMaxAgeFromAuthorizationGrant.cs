using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuthServer.TestIdentityProvider.Migrations
{
    /// <inheritdoc />
    public partial class RemoveMaxAgeFromAuthorizationGrant : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaxAge",
                table: "AuthorizationGrant");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "MaxAge",
                table: "AuthorizationGrant",
                type: "bigint",
                nullable: true);
        }
    }
}
