using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    public partial class SetExplicitRelationsInClient : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ClientGrant_Clients_ClientsId",
                table: "ClientGrant");

            migrationBuilder.DropForeignKey(
                name: "FK_ClientGrant_Grants_GrantsId",
                table: "ClientGrant");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ClientGrant",
                table: "ClientGrant");

            migrationBuilder.RenameTable(
                name: "ClientGrant",
                newName: "ClientGrants");

            migrationBuilder.RenameIndex(
                name: "IX_ClientGrant_GrantsId",
                table: "ClientGrants",
                newName: "IX_ClientGrants_GrantsId");

            migrationBuilder.AddColumn<string>(
                name: "TosUri",
                table: "Clients",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ClientGrants",
                table: "ClientGrants",
                columns: new[] { "ClientsId", "GrantsId" });

            migrationBuilder.AddForeignKey(
                name: "FK_ClientGrants_Clients_ClientsId",
                table: "ClientGrants",
                column: "ClientsId",
                principalTable: "Clients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ClientGrants_Grants_GrantsId",
                table: "ClientGrants",
                column: "GrantsId",
                principalTable: "Grants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ClientGrants_Clients_ClientsId",
                table: "ClientGrants");

            migrationBuilder.DropForeignKey(
                name: "FK_ClientGrants_Grants_GrantsId",
                table: "ClientGrants");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ClientGrants",
                table: "ClientGrants");

            migrationBuilder.DropColumn(
                name: "TosUri",
                table: "Clients");

            migrationBuilder.RenameTable(
                name: "ClientGrants",
                newName: "ClientGrant");

            migrationBuilder.RenameIndex(
                name: "IX_ClientGrants_GrantsId",
                table: "ClientGrant",
                newName: "IX_ClientGrant_GrantsId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ClientGrant",
                table: "ClientGrant",
                columns: new[] { "ClientsId", "GrantsId" });

            migrationBuilder.AddForeignKey(
                name: "FK_ClientGrant_Clients_ClientsId",
                table: "ClientGrant",
                column: "ClientsId",
                principalTable: "Clients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ClientGrant_Grants_GrantsId",
                table: "ClientGrant",
                column: "GrantsId",
                principalTable: "Grants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
