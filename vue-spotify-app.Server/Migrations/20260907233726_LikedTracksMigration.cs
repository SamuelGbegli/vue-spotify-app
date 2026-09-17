using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace vue_spotify_app.Server.Migrations
{
    /// <inheritdoc />
    public partial class LikedTracksMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "TrackListID",
                table: "TrackRecords",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TrackLists",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SortName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserID = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TrackListType = table.Column<int>(type: "int", nullable: false),
                    PlaylistID = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrackLists", x => x.ID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TrackRecords_TrackListID",
                table: "TrackRecords",
                column: "TrackListID");

            migrationBuilder.AddForeignKey(
                name: "FK_TrackRecords_TrackLists_TrackListID",
                table: "TrackRecords",
                column: "TrackListID",
                principalTable: "TrackLists",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TrackRecords_TrackLists_TrackListID",
                table: "TrackRecords");

            migrationBuilder.DropTable(
                name: "TrackLists");

            migrationBuilder.DropIndex(
                name: "IX_TrackRecords_TrackListID",
                table: "TrackRecords");

            migrationBuilder.DropColumn(
                name: "TrackListID",
                table: "TrackRecords");
        }
    }
}
