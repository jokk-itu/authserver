using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    public partial class AddResourceUriColumn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Uri",
                table: "Resource",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Resource_Name",
                table: "Resource",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Resource_Uri",
                table: "Resource",
                column: "Uri",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Resource_Name",
                table: "Resource");

            migrationBuilder.DropIndex(
                name: "IX_Resource_Uri",
                table: "Resource");

            migrationBuilder.DropColumn(
                name: "Uri",
                table: "Resource");
        }
    }
}
