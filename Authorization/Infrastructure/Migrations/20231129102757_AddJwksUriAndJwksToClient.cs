using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    public partial class AddJwksUriAndJwksToClient : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Jwks",
                table: "Client",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "JwksUri",
                table: "Client",
                type: "TEXT",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Jwks",
                table: "Client");

            migrationBuilder.DropColumn(
                name: "JwksUri",
                table: "Client");
        }
    }
}
