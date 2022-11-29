using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    public partial class AddApplicationTypetoClients : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClientProfile",
                table: "Clients");

            migrationBuilder.RenameColumn(
                name: "ClientType",
                table: "Clients",
                newName: "ApplicationType");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ApplicationType",
                table: "Clients",
                newName: "ClientType");

            migrationBuilder.AddColumn<string>(
                name: "ClientProfile",
                table: "Clients",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
