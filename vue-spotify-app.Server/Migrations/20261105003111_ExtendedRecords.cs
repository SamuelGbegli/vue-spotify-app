using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace vue_spotify_app.Server.Migrations
{
    /// <inheritdoc />
    public partial class ExtendedRecords : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SavedTracks");

            migrationBuilder.CreateTable(
                name: "ExtendedRecords",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TrackURI = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AmountPlayed = table.Column<int>(type: "int", nullable: false),
                    Shuffle = table.Column<bool>(type: "bit", nullable: false),
                    OfflineTimestamp = table.Column<int>(type: "int", nullable: true),
                    ReasonStart = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReasonEnd = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Platform = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExtendedRecords", x => x.ID);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExtendedRecords");

            migrationBuilder.CreateTable(
                name: "SavedTracks",
                columns: table => new
                {
                    ID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DateAdded = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SpotifyID = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SavedTracks", x => x.ID);
                    table.ForeignKey(
                        name: "FK_SavedTracks_Users_UserID",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SavedTracks_UserID",
                table: "SavedTracks",
                column: "UserID");
        }
    }
}
