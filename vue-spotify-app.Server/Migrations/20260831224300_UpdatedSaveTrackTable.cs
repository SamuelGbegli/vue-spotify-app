using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace vue_spotify_app.Server.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedSaveTrackTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {


            migrationBuilder.AlterColumn<Guid>(
                name: "UserID",
                table: "SavedTracks",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_SavedTracks_UserID",
                table: "SavedTracks",
                column: "UserID");

            migrationBuilder.AddForeignKey(
                name: "FK_SavedTracks_Users_UserID",
                table: "SavedTracks",
                column: "UserID",
                principalTable: "Users",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SavedTracks_Users_UserID",
                table: "SavedTracks");

            migrationBuilder.DropIndex(
                name: "IX_SavedTracks_UserID",
                table: "SavedTracks");

            migrationBuilder.AlterColumn<string>(
                name: "UserID",
                table: "SavedTracks",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<Guid>(
                name: "UserID1",
                table: "SavedTracks",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SavedTracks_UserID1",
                table: "SavedTracks",
                column: "UserID1");

            migrationBuilder.AddForeignKey(
                name: "FK_SavedTracks_Users_UserID1",
                table: "SavedTracks",
                column: "UserID1",
                principalTable: "Users",
                principalColumn: "ID");
        }
    }
}
