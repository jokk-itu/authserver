using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    public partial class AddClientCredentialsGrantType : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "GrantTypes",
                columns: new[] { "Id", "Name" },
                values: new object[] { 3, "client_credentials" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "GrantTypes",
                keyColumn: "Id",
                keyValue: 3);
        }
    }
}
