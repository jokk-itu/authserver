using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuthServer.TestIdentityProvider.Migrations
{
    /// <inheritdoc />
    public partial class RemovePairwiseSubject : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AuthorizationGrant_SubjectIdentifier_SubjectIdentifierId",
                table: "AuthorizationGrant");

            migrationBuilder.DropForeignKey(
                name: "FK_ConsentGrant_SubjectIdentifier_PublicSubjectIdentifierId",
                table: "ConsentGrant");

            migrationBuilder.DropForeignKey(
                name: "FK_Session_SubjectIdentifier_PublicSubjectIdentifierId",
                table: "Session");

            migrationBuilder.DropForeignKey(
                name: "FK_SubjectIdentifier_Client_ClientId",
                table: "SubjectIdentifier");

            migrationBuilder.DropForeignKey(
                name: "FK_SubjectIdentifier_SubjectIdentifier_PublicSubjectIdentifierId",
                table: "SubjectIdentifier");

            migrationBuilder.DropIndex(
                name: "IX_SubjectIdentifier_ClientId",
                table: "SubjectIdentifier");

            migrationBuilder.DropIndex(
                name: "IX_SubjectIdentifier_PublicSubjectIdentifierId",
                table: "SubjectIdentifier");

            migrationBuilder.DropIndex(
                name: "IX_AuthorizationGrant_SubjectIdentifierId",
                table: "AuthorizationGrant");

            migrationBuilder.DropColumn(
                name: "ClientId",
                table: "SubjectIdentifier");

            migrationBuilder.DropColumn(
                name: "PublicSubjectIdentifierId",
                table: "SubjectIdentifier");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "SubjectIdentifier");

            migrationBuilder.DropColumn(
                name: "SubjectIdentifierId",
                table: "AuthorizationGrant");

            migrationBuilder.RenameColumn(
                name: "PublicSubjectIdentifierId",
                table: "Session",
                newName: "SubjectIdentifierId");

            migrationBuilder.RenameIndex(
                name: "IX_Session_PublicSubjectIdentifierId",
                table: "Session",
                newName: "IX_Session_SubjectIdentifierId");

            migrationBuilder.RenameColumn(
                name: "PublicSubjectIdentifierId",
                table: "ConsentGrant",
                newName: "SubjectIdentifierId");

            migrationBuilder.RenameIndex(
                name: "IX_ConsentGrant_PublicSubjectIdentifierId",
                table: "ConsentGrant",
                newName: "IX_ConsentGrant_SubjectIdentifierId");

            migrationBuilder.AddColumn<int>(
                name: "SectorIdentifierId",
                table: "Client",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Subject",
                table: "AuthorizationGrant",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "SectorIdentifier",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Uri = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Salt = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SectorIdentifier", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Client_SectorIdentifierId",
                table: "Client",
                column: "SectorIdentifierId");

            migrationBuilder.CreateIndex(
                name: "IX_SectorIdentifier_Uri",
                table: "SectorIdentifier",
                column: "Uri",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Client_SectorIdentifier_SectorIdentifierId",
                table: "Client",
                column: "SectorIdentifierId",
                principalTable: "SectorIdentifier",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ConsentGrant_SubjectIdentifier_SubjectIdentifierId",
                table: "ConsentGrant",
                column: "SubjectIdentifierId",
                principalTable: "SubjectIdentifier",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Session_SubjectIdentifier_SubjectIdentifierId",
                table: "Session",
                column: "SubjectIdentifierId",
                principalTable: "SubjectIdentifier",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Client_SectorIdentifier_SectorIdentifierId",
                table: "Client");

            migrationBuilder.DropForeignKey(
                name: "FK_ConsentGrant_SubjectIdentifier_SubjectIdentifierId",
                table: "ConsentGrant");

            migrationBuilder.DropForeignKey(
                name: "FK_Session_SubjectIdentifier_SubjectIdentifierId",
                table: "Session");

            migrationBuilder.DropTable(
                name: "SectorIdentifier");

            migrationBuilder.DropIndex(
                name: "IX_Client_SectorIdentifierId",
                table: "Client");

            migrationBuilder.DropColumn(
                name: "SectorIdentifierId",
                table: "Client");

            migrationBuilder.DropColumn(
                name: "Subject",
                table: "AuthorizationGrant");

            migrationBuilder.RenameColumn(
                name: "SubjectIdentifierId",
                table: "Session",
                newName: "PublicSubjectIdentifierId");

            migrationBuilder.RenameIndex(
                name: "IX_Session_SubjectIdentifierId",
                table: "Session",
                newName: "IX_Session_PublicSubjectIdentifierId");

            migrationBuilder.RenameColumn(
                name: "SubjectIdentifierId",
                table: "ConsentGrant",
                newName: "PublicSubjectIdentifierId");

            migrationBuilder.RenameIndex(
                name: "IX_ConsentGrant_SubjectIdentifierId",
                table: "ConsentGrant",
                newName: "IX_ConsentGrant_PublicSubjectIdentifierId");

            migrationBuilder.AddColumn<string>(
                name: "ClientId",
                table: "SubjectIdentifier",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PublicSubjectIdentifierId",
                table: "SubjectIdentifier",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "SubjectIdentifier",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "SubjectIdentifierId",
                table: "AuthorizationGrant",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_SubjectIdentifier_ClientId",
                table: "SubjectIdentifier",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_SubjectIdentifier_PublicSubjectIdentifierId",
                table: "SubjectIdentifier",
                column: "PublicSubjectIdentifierId");

            migrationBuilder.CreateIndex(
                name: "IX_AuthorizationGrant_SubjectIdentifierId",
                table: "AuthorizationGrant",
                column: "SubjectIdentifierId");

            migrationBuilder.AddForeignKey(
                name: "FK_AuthorizationGrant_SubjectIdentifier_SubjectIdentifierId",
                table: "AuthorizationGrant",
                column: "SubjectIdentifierId",
                principalTable: "SubjectIdentifier",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ConsentGrant_SubjectIdentifier_PublicSubjectIdentifierId",
                table: "ConsentGrant",
                column: "PublicSubjectIdentifierId",
                principalTable: "SubjectIdentifier",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Session_SubjectIdentifier_PublicSubjectIdentifierId",
                table: "Session",
                column: "PublicSubjectIdentifierId",
                principalTable: "SubjectIdentifier",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SubjectIdentifier_Client_ClientId",
                table: "SubjectIdentifier",
                column: "ClientId",
                principalTable: "Client",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SubjectIdentifier_SubjectIdentifier_PublicSubjectIdentifierId",
                table: "SubjectIdentifier",
                column: "PublicSubjectIdentifierId",
                principalTable: "SubjectIdentifier",
                principalColumn: "Id");
        }
    }
}
